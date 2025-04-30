using System.Collections.Generic;

namespace AzureDevopsPullrequestMcpServer.Models;

public class PullRequestDetails
{
    public int PullRequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string SourceBranchRef { get;  set; } = string.Empty;
    public string TragetBranchRef { get; set; } = string.Empty;
}
