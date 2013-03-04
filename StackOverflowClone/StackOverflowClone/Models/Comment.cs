using System;

namespace StackOverflowClone.Models
{
    public class Comment
    {
        public string Content { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedByUserId { get; set; }
        public int UpVoteCount { get; set; }
    }
}