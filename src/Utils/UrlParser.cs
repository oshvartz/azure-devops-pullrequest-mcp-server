using AzureDevopsPullrequestMcpServer.Models;

namespace AzureDevopsPullrequestMcpServer.Utils;

public class UrlParser : IUrlParser
{
    public PullRequestUrlInfo ParsePullRequestUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        // Example URL format: https://dev.azure.com/{organization}/{project}/_git/{repository}/pullrequest/{pullRequestId}
        // or https://{organization}.visualstudio.com/{project}/_git/{repository}/pullrequest/{pullRequestId}

        var uri = new Uri(url);
        string organization;
        string project;
        int pullRequestId;

        if (uri.Host.EndsWith("visualstudio.com"))
        {
            // Parse organization.visualstudio.com format
            organization = uri.Host.Split('.')[0];
        }
        else if (uri.Host == "dev.azure.com")
        {
            // Parse dev.azure.com format
            organization = uri.Segments[1].TrimEnd('/');
        }
        else
        {
            throw new ArgumentException("Invalid Azure DevOps URL format", nameof(url));
        }

        // Extract project name and PR ID from path segments
        var segments = uri.Segments.Select(s => s.TrimEnd('/')).ToList();
        var prIndex = Array.IndexOf(segments.ToArray(), "pullrequest");
        var gitIndex = Array.IndexOf(segments.ToArray(), "_git");
        
        if (prIndex == -1 || prIndex + 1 >= segments.Count)
            throw new ArgumentException("Could not find pull request ID in URL", nameof(url));

        if (gitIndex == -1)
            throw new ArgumentException("Could not find _git in URL", nameof(url));

        // Project name is the segment before _git
        project = segments[gitIndex - 1];
        
        // Repository name is the segment after _git
        string repository = segments[gitIndex + 1];
        
        if (!int.TryParse(segments[prIndex + 1], out pullRequestId))
            throw new ArgumentException("Invalid pull request ID format", nameof(url));

        return new PullRequestUrlInfo
        {
            Organization = organization,
            Project = project,
            Repository = repository,
            PullRequestId = pullRequestId
        };
    }
}
