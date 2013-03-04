using System;
using System.Collections.Generic;

namespace StackOverflowClone.Core
{
    public class QuestionLightViewModel
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public ICollection<string> Tags { get; set; }
        public string CreatedBy { get; set; }
        public int UserReputation { get; set; }
        public int AnswersCount { get; set; }
        public int ViewsCount { get; set; }
        public int TotalVotes { get; set; }
    }
}