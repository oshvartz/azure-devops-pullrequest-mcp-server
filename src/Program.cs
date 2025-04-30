using AzureDevopsPullrequestMcpServer.Services;
using AzureDevopsPullrequestMcpServer.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using System.ComponentModel;
using static Microsoft.Azure.Pipelines.WebApi.PipelinesResources;

namespace AzureDevopsPullrequestMcpServer;

public class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            // Run as MCP server when no arguments provided
            await RunMcpServerMode();
        }
        else
        {
            // Run as CLI tool with the PR URL
            await RunCliMode(args[0]);
        }
    }

    private static async Task RunMcpServerMode()
    {
        try
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Logging.AddConsole(consoleLogOptions =>
            {
                // Configure all logs to go to stderr
                consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
            });

            builder.Services.AddSingleton<IUrlParser, UrlParser>();
            builder.Services.AddSingleton<IAzureDevOpsClient, AzureDevOpsClient>();

            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

            await builder.Build().RunAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"MCP Server error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task RunCliMode(string prUrl)
    {
        var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        if (string.IsNullOrWhiteSpace(pat))
        {
            Console.Error.WriteLine("Error: AZURE_DEVOPS_PAT environment variable is required for authentication");
            return;
        }

        Console.WriteLine("Azure DevOps PR Details Fetcher");
        Console.WriteLine("------------------------------");

        if (string.IsNullOrWhiteSpace(prUrl))
        {
            Console.WriteLine("Error: PR URL is required");
            Console.WriteLine("Usage: AzureDevopsPullrequestMcpServer <PR_URL>");
            Console.WriteLine("Example: AzureDevopsPullrequestMcpServer https://dev.azure.com/org/project/_git/repo/pullrequest/123");
            return;
        }

        try
        {
            var urlParser = new UrlParser();
            var prInfo = urlParser.ParsePullRequestUrl(prUrl);

            var client = new AzureDevOpsClient();
            await client.InitializeConnectionAsync(prInfo.Organization, pat);

            var res = await client.CreateThreadAsync(prInfo.Project, prInfo.Repository, prInfo.PullRequestId, "test context new2 ", new Models.AdoThreadContext { FilePath = @"src/AgentServices/AgentPoliciesApi/Core/Monitoring/Logs/UserAssignmentSyncOperationLogMessage.cs", StartLine = 54, StartOffset = 0, EndLine = 54, EndOffset = 30});
                                                                                                                                                                                            
            var prDetails = await client.GetPullRequestAsync(prInfo.Project, prInfo.Repository, prInfo.PullRequestId);
            var threads = await client.GetFilteredPullRequestThreadsAsync(prInfo.Project, prInfo.Repository, prInfo.PullRequestId);  
            Console.WriteLine("\nPull Request Details:");
            Console.WriteLine($"ID: {prDetails.PullRequestId}");
            Console.WriteLine($"Title: {prDetails.Title}");
            Console.WriteLine($"Creator: {prDetails.Creator}");
            Console.WriteLine($"Status: {prDetails.Status}");
            Console.WriteLine($"Created: {prDetails.CreatedDate:g}");
            Console.WriteLine("\nDescription:");
            Console.WriteLine(prDetails.Description);

            if (threads.Any())
            {
                Console.WriteLine("\nThread Comments:");
                foreach (var thread in threads)
                {
                    Console.WriteLine($"\nThread {thread.ThreadId} (Status: {thread.Status})");
                    var firstTwoComments = thread.GetFirstTwoComments();
                    foreach (var comment in firstTwoComments)
                    {
                        Console.WriteLine($"- {comment.Author} ({comment.CreatedDate:g}):");
                        Console.WriteLine($"  {comment.Content}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
