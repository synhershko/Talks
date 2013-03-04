using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StackOverflowClone.Models;

namespace StackOverflowClone.Core
{
    public class QuestionInputModel
    {
        [Required]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Tags { get; set; }

        public Question ToQuestion()
        {
            var question = new Question
                               {
                                   CreatedOn = DateTimeOffset.UtcNow,
                                   CreatedBy = "users/1",
                                   Answers = new List<Answer>(),
                                   Comments = new List<Comment>(),
                                   Content = Content,
                                   Subject = Subject,
                                   Tags =
                                       (Tags ?? string.Empty).Split(new[] {' ', ','},
                                                                    StringSplitOptions.RemoveEmptyEntries)
                               };
            return question;
        }
    }
}