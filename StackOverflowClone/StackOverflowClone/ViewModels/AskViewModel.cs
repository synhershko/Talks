using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using StackOverflowClone.Core;
using StackOverflowClone.Models;

namespace StackOverflowClone.ViewModels
{
    public class AskViewModel
    {
        public AskViewModel(IPrincipal user)
        {
            User = new UserViewModel(user);
            Question = new QuestionInputModel();
        }

        public UserViewModel User { get; set; }
        public QuestionInputModel Question { get; set; }
    }
}