using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsPullrequestMcpServer.Models;

public class PrDetailsInput
{
    [Required(ErrorMessage = "PR URL is required")]
    [Display(Description = "Azure DevOps pull request URL")]
    public string PrUrl { get; set; } = string.Empty;

    [Display(Description = "Filter threads to include only these statuses (e.g., ['active', 'closed'])")]
    public string[]? IncludeStatuses { get; set; }

    [Display(Description = "Filter threads to exclude these statuses (e.g., ['pending'])")]
    public string[]? ExcludeStatuses { get; set; }
}
