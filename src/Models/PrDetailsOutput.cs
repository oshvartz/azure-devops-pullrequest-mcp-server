using System.ComponentModel.DataAnnotations;

namespace AzureDevopsPullrequestMcpServer.Models;

public class PrDetailsOutput
{
    [Display(Description = "Pull request details")]
    public PullRequestDetails PullRequest { get; set; } = new();
}
