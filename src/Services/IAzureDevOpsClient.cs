using AzureDevopsPullrequestMcpServer.Models;

namespace AzureDevopsPullrequestMcpServer.Services;

public interface IAzureDevOpsClient
{
    Task InitializeConnectionAsync(string organization, string pat);
    Task<List<PullRequestThread>> GetPullRequestThreadsAsync(string project, string repository, int pullRequestId);
    Task<List<PullRequestThread>> GetFilteredPullRequestThreadsAsync(string project, string repository, int pullRequestId, string[]? includeStatuses = null, string[]? excludeStatuses = null);
    Task<PullRequestDetails> GetPullRequestAsync(string project, string repository, int pullRequestId);
    Task<PullRequestThread> CreateThreadAsync(string project, string repository, int pullRequestId, string content, AdoThreadContext context);
}
