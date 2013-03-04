using System;
using System.Collections.Generic;

namespace StackOverflowClone.Models
{
    public class Answer
    {
        public string CreatedByUserId { get; set; }
        public string Content { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? LastEditedOn { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public Stats Stats { get; set; } // For the answer, we actually are going to store stats right here
    }
}