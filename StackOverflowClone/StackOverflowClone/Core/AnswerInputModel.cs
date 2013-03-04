using System.ComponentModel.DataAnnotations;

namespace StackOverflowClone.Core
{
    public class AnswerInputModel
    {
        [Required]
        public string Content { get; set; }
    }
}