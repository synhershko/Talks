using System;
using System.Collections.Generic;
using Raven.Imports.Newtonsoft.Json;

namespace StackOverflowClone.Models
{
    public class Question
    {
        public Question()
        {
            CreatedOn = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public ICollection<string> Tags { get; set; }
        public string CreatedBy { get; set; }
        public ICollection<Answer> Answers { get; set; }
        public ICollection<Comment> Comments { get; set; }

        [JsonIgnore] // We are not going to store this, we just need a to move it around along with the question object
        public Stats Stats { get; set; }
    }
}