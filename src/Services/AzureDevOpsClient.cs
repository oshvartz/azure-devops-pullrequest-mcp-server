using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using AzureDevopsPullrequestMcpServer.Models;
using Microsoft.Azure.Pipelines.WebApi;
using System.IO;

namespace AzureDevopsPullrequestMcpServer.Services;

public class AzureDevOpsClient : IAzureDevOpsClient
{
    private VssConnection? _connection;

    public async Task InitializeConnectionAsync(string organization, string pat)
    {
        var uri = new Uri($"https://dev.azure.com/{organization}");
        _connection = new VssConnection(uri, new VssBasicCredential(string.Empty, pat));
        await _connection.ConnectAsync();
    }

    public async Task<List<PullRequestThread>> GetPullRequestThreadsAsync(string project, string repository, int pullRequestId)
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection not initialized. Call InitializeConnectionAsync first.");

        var gitClient = _connection.GetClient<GitHttpClient>();
        var threads = await gitClient.GetThreadsAsync(project, repository, pullRequestId);
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

    public async Task<List<PullRequestThread>> GetFilteredPullRequestThreadsAsync(string project, string repository, int pullRequestId, string[]? includeStatuses = null, string[]? excludeStatuses = null)
    {
        var threads = await GetPullRequestThreadsAsync(project, repository, pullRequestId);

        if (includeStatuses?.Length > 0)
            threads = threads.Where(t => includeStatuses.Contains(t.Status, StringComparer.OrdinalIgnoreCase)).ToList();

        if (excludeStatuses?.Length > 0)
            threads = threads.Where(t => !excludeStatuses.Contains(t.Status, StringComparer.OrdinalIgnoreCase)).ToList();

        return threads;
    }

    public async Task<PullRequestDetails> GetPullRequestAsync(string project, string repository, int pullRequestId)
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection not initialized. Call InitializeConnectionAsync first.");

        var gitClient = _connection.GetClient<GitHttpClient>();
        var pullRequest = await gitClient.GetPullRequestByIdAsync(pullRequestId);

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

    public async Task<PullRequestThread> CreateThreadAsync(string project, string repository, int pullRequestId, string content, AdoThreadContext context)
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection not initialized. Call InitializeConnectionAsync first.");

        if(context.FilePath is not null)
        {
            context.FilePath = context.FilePath.StartsWith("/") ? context.FilePath : "/" + context.FilePath;
        }
        
        var gitClient = _connection.GetClient<GitHttpClient>();

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

        var createdThread = await gitClient.CreateThreadAsync(thread, project, repository, pullRequestId);
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
}
