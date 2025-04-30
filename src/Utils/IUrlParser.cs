using AzureDevopsPullrequestMcpServer.Models;

namespace AzureDevopsPullrequestMcpServer.Utils;

public interface IUrlParser
{
    PullRequestUrlInfo ParsePullRequestUrl(string url);
}
