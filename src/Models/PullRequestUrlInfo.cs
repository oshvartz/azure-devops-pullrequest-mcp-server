namespace AzureDevopsPullrequestMcpServer.Models;

public class PullRequestUrlInfo
{
    public string Organization { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public int PullRequestId { get; set; }
}
