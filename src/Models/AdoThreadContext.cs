using System.ComponentModel.DataAnnotations;

namespace AzureDevopsPullrequestMcpServer.Models;

public class AdoThreadContext
{
    [Required(ErrorMessage = "File path is required")]
    [Display(Description = "Path to the file where the comment should be placed")]
    public string FilePath { get; set; } = string.Empty;

    [Display(Description = "The line number in the source file where you want comment to refrence to start with. Starts at 1.")]
    public int? StartLine { get; set; }

    [Display(Description = "for the start line in the file: the character offset of a thread's position inside of a line. Starts at 0.")]
    public int? StartOffset { get; set; }

    [Display(Description = "The line number in the source file where you want comment to refrence to - to end")]
    public int? EndLine { get; set; }

    [Display(Description = "for the end line in the file: The character offset of a thread's position inside of a line. Starts at 0.")]
    public int? EndOffset { get; set; }

}
