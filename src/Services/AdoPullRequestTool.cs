using System.ComponentModel;
using AzureDevopsPullrequestMcpServer.Models;
using AzureDevopsPullrequestMcpServer.Utils;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace AzureDevopsPullrequestMcpServer.Services;

[McpServerToolType]
public static class AdoPullRequestTool
{
    [McpServerTool, Description("Get pull request details from Azure DevOps")]
    public static async Task<PrDetailsOutput> GetPrDetails(PrDetailsInput input, IUrlParser urlParser, IAzureDevOpsClient client)
    {
        var prInfo = urlParser.ParsePullRequestUrl(input.PrUrl);
            
        await client.InitializeConnectionAsync(prInfo.Organization);

        var prDetails = await client.GetPullRequestAsync(prInfo.Project, prInfo.Repository, prInfo.PullRequestId);

        return new PrDetailsOutput
        {
            PullRequest = prDetails
        };
    }

    [McpServerTool, Description("Get pull request threads from Azure DevOps")]
    public static async Task<ThreadsOutput> GetPrThreads(PrDetailsInput input, IUrlParser urlParser, IAzureDevOpsClient client)
    {
        var prInfo = urlParser.ParsePullRequestUrl(input.PrUrl);
            
        await client.InitializeConnectionAsync(prInfo.Organization);

        var threads = await client.GetFilteredPullRequestThreadsAsync(
            prInfo.Project,
            prInfo.Repository,
            prInfo.PullRequestId,
            input.IncludeStatuses,
            input.ExcludeStatuses);

        return new ThreadsOutput
        {
            Threads = threads.ToArray()
        };
    }

    [McpServerTool, Description("Create a comment thread on a specific location in code")]
    public static async Task<ThreadOutput> CreatePrThread(AdoCreateThreadInput input, IUrlParser urlParser, IAzureDevOpsClient client)
    {
        var prInfo = urlParser.ParsePullRequestUrl(input.PrUrl);
            
        await client.InitializeConnectionAsync(prInfo.Organization);

        var thread = await client.CreateThreadAsync(
            prInfo.Project,
            prInfo.Repository,
            prInfo.PullRequestId,
            input.Content,
            input.Context);

        return new ThreadOutput
        {
            Thread = thread
        };
    }
}
