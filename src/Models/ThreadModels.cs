using System;
using System.Collections.Generic;

namespace AzureDevopsPullrequestMcpServer.Models;

public class ThreadComment
{
    public int CommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}

public class PullRequestThread
{
    public int ThreadId { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ThreadComment> Comments { get; set; } = new List<ThreadComment>();

    public IEnumerable<ThreadComment> GetFirstTwoComments()
    {
        return Comments.Take(2);
    }
}
