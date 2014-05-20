using System.Collections.Generic;
using System.Security.Principal;
using StackOverflowClone.Core;
using StackOverflowClone.Models;

namespace StackOverflowClone.ViewModels
{
    public class ViewQuestionViewModel
    {
        public ViewQuestionViewModel(IPrincipal user)
        {
            User = new UserViewModel(user);            
        }

        public UserViewModel User { get; set; }
        public Question Question { get; set; }
        public Dictionary<string, User> Users { get; set; }
        public Question[] RelatedQuestions { get; set; }
    }
}