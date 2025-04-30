using System.ComponentModel.DataAnnotations;

namespace AzureDevopsPullrequestMcpServer.Models;

public class ThreadsOutput
{
    [Display(Description = "Pull request threads")]
    public PullRequestThread[] Threads { get; set; } = Array.Empty<PullRequestThread>();
}
