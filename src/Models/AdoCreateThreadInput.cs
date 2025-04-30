using System.ComponentModel.DataAnnotations;

namespace AzureDevopsPullrequestMcpServer.Models;

public class AdoCreateThreadInput
{
    [Required(ErrorMessage = "PR URL is required")]
    [Display(Description = "Azure DevOps pull request URL")]
    public string PrUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Comment content is required")]
    [Display(Description = "Content of the comment to be created")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Thread context is required")]
    [Display(Description = "Context information about where to place the comment")]
    public AdoThreadContext Context { get; set; } = new();
}
