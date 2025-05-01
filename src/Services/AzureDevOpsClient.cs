using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using AzureDevopsPullrequestMcpServer.Models;
using Microsoft.Azure.Pipelines.WebApi;
using System.IO;
using Microsoft.VisualStudio.Services.Client;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace AzureDevopsPullrequestMcpServer.Services;

public class AzureDevOpsClient : IAzureDevOpsClient
{
    private const string AZURE_DEVOPS_SCORE = "499b84ac-1321-427f-aa17-267ca6975798/.default";
    private readonly ILogger<AzureDevOpsClient> _logger;
    private IVssConnection? _connection;
    private readonly Lazy<Task<DefaultAzureCredential>> _defaultAzureCredential;

    public AzureDevOpsClient(ILogger<AzureDevOpsClient> logger)
    {
        _logger = logger;
        string pat = GetPatToken();
        if(string.IsNullOrEmpty(pat))
        {
            _defaultAzureCredential = new Lazy<Task<DefaultAzureCredential>>(() => InitializeCredAsync());
            _ = _defaultAzureCredential.Value;
        }
        else
        {
            _defaultAzureCredential = null!;
        }
    }

    private async Task<DefaultAzureCredential> InitializeCredAsync()
    {
        var cred = new DefaultAzureCredential();
        _logger.LogDebug("Attempting to GetTokenAsync to Azure DevOps...");
        await cred.GetTokenAsync(new Azure.Core.TokenRequestContext(new string[] { AZURE_DEVOPS_SCORE }));
        _logger.LogDebug("Completed GetTokenAsync to Azure DevOps...");
        return cred;
    }

    public async Task InitializeConnectionAsync(string organization)
    {
        try
        {
            _logger.LogInformation("Initializing Azure DevOps connection for organization: {Organization}", organization);

            var uri = new Uri($"https://dev.azure.com/{organization}");
            string pat = GetPatToken();
            if (string.IsNullOrEmpty(pat))
            {
                _logger.LogInformation("pat is empty using DefaultAzureCredential");
                var cred = new VssAzureIdentityCredential(await _defaultAzureCredential.Value);
                    
                _connection = new VssConnection(uri, cred);
            }
            else
            {
                _connection = new VssConnection(uri, new VssBasicCredential(string.Empty, pat));
            }
            _logger.LogDebug("Attempting to connect to Azure DevOps...");
            await _connection.ConnectAsync();
            _logger.LogInformation("Successfully connected to Azure DevOps");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Azure DevOps connection: {Message}", ex.Message);
            throw;
        }
    }

    private static string GetPatToken()
    {
        return Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT") ?? string.Empty;
    }

    public async Task<List<PullRequestThread>> GetPullRequestThreadsAsync(string project, string repository, int pullRequestId)
    {
        try
        {
            _logger.LogInformation("Getting PR threads for PR {PullRequestId} in {Project}/{Repository}", pullRequestId, project, repository);
            
            if (_connection == null)
            {
                _logger.LogError("Connection not initialized. Call InitializeConnectionAsync first");
                throw new InvalidOperationException("Connection not initialized. Call InitializeConnectionAsync first.");
            }

            var gitClient = _connection.GetClient<GitHttpClient>();
            _logger.LogDebug("Fetching threads from Azure DevOps API...");
            var threads = await gitClient.GetThreadsAsync(project, repository, pullRequestId);
            _logger.LogInformation("Successfully retrieved {Count} threads", threads.Count);
            return threads.Select(t => new PullRequestThread
            {
                ThreadId = t.Id,
                Status = t.Status.ToString(),
                Comments = t.Comments.Select(c => new ThreadComment
                {
                    CommentId = c.Id,
                    Content = c.Content ?? string.Empty,
                    Author = c.Author.DisplayName,
                    CreatedDate = c.PublishedDate,
                    LastUpdatedDate = c.LastUpdatedDate
                }).ToList()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PR threads: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<List<PullRequestThread>> GetFilteredPullRequestThreadsAsync(string project, string repository, int pullRequestId, string[]? includeStatuses = null, string[]? excludeStatuses = null)
    {
        try
        {
            _logger.LogInformation("Getting filtered PR threads with includeStatuses: {IncludeStatuses}, excludeStatuses: {ExcludeStatuses}",
                includeStatuses != null ? string.Join(", ", includeStatuses) : "none",
                excludeStatuses != null ? string.Join(", ", excludeStatuses) : "none");

            var threads = await GetPullRequestThreadsAsync(project, repository, pullRequestId);

            if (includeStatuses?.Length > 0)
            {
                _logger.LogDebug("Filtering threads by included statuses");
                threads = threads.Where(t => includeStatuses.Contains(t.Status, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            if (excludeStatuses?.Length > 0)
            {
                _logger.LogDebug("Filtering threads by excluded statuses");
                threads = threads.Where(t => !excludeStatuses.Contains(t.Status, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            _logger.LogInformation("Returning {Count} filtered threads", threads.Count);
            return threads;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering PR threads: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PullRequestDetails> GetPullRequestAsync(string project, string repository, int pullRequestId)
    {
        try
        {
            _logger.LogInformation("Getting PR details for PR {PullRequestId} in {Project}/{Repository}", pullRequestId, project, repository);

            if (_connection == null)
            {
                _logger.LogError("Connection not initialized. Call InitializeConnectionAsync first");
                throw new InvalidOperationException("Connection not initialized. Call InitializeConnectionAsync first.");
            }

            var gitClient = _connection.GetClient<GitHttpClient>();
            _logger.LogDebug("Fetching PR details from Azure DevOps API...");
            var pullRequest = await gitClient.GetPullRequestByIdAsync(pullRequestId);
            _logger.LogDebug("Successfully retrieved PR details");

            return new PullRequestDetails
            {
                PullRequestId = pullRequest.PullRequestId,
                Title = pullRequest.Title,
                Description = pullRequest.Description ?? string.Empty,
                Status = pullRequest.Status.ToString(),
                Creator = pullRequest.CreatedBy.DisplayName,
                CreatedDate = pullRequest.CreationDate.Date,
                SourceBranchRef = pullRequest.SourceRefName,
                TragetBranchRef = pullRequest.TargetRefName,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PR details: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PullRequestThread> CreateThreadAsync(string project, string repository, int pullRequestId, string content, AdoThreadContext context)
    {
        try
        {
            _logger.LogInformation("Creating PR thread for PR {PullRequestId} in {Project}/{Repository}", pullRequestId, project, repository);

            if (_connection == null)
            {
                _logger.LogError("Connection not initialized. Call InitializeConnectionAsync first");
                throw new InvalidOperationException("Connection not initialized. Call InitializeConnectionAsync first.");
            }

            if(context.FilePath is not null)
            {
                context.FilePath = context.FilePath.StartsWith("/") ? context.FilePath : "/" + context.FilePath;
            }
            
            _logger.LogDebug("Creating Git client...");
            var gitClient = _connection.GetClient<GitHttpClient>();

            _logger.LogDebug("Building thread with file path: {FilePath}", context.FilePath);
            var thread = new GitPullRequestCommentThread
        {
            Comments = new List<Microsoft.TeamFoundation.SourceControl.WebApi.Comment>
            {
                new() { Content = content }
            },
            ThreadContext = new CommentThreadContext
            {
                FilePath = context.FilePath,
                RightFileStart = new CommentPosition
                {
                    Line = context.StartLine ?? 1,
                    Offset = context.StartOffset is not null && context.StartOffset.Value > 0 ? context.StartOffset.Value : 1,
                },
                RightFileEnd = context.EndLine.HasValue ? new CommentPosition
                {
                    Line = context.EndLine ?? 1,
                    Offset = context.EndOffset is not null && context.EndOffset.Value > 0 ? context.EndOffset.Value : 1,
                } : null,
            }
        };

            _logger.LogDebug("Creating thread in Azure DevOps...");
            var createdThread = await gitClient.CreateThreadAsync(thread, project, repository, pullRequestId);
            _logger.LogInformation("Successfully created thread {ThreadId}", createdThread.Id);

            return new PullRequestThread
            {
                ThreadId = createdThread.Id,
                Status = createdThread.Status.ToString(),
                Comments = createdThread.Comments.Select(c => new ThreadComment
                {
                    CommentId = c.Id,
                    Content = c.Content ?? string.Empty,
                    Author = c.Author.DisplayName,
                    CreatedDate = c.PublishedDate,
                    LastUpdatedDate = c.LastUpdatedDate
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PR thread: {Message}", ex.Message);
            throw;
        }
    }
}
