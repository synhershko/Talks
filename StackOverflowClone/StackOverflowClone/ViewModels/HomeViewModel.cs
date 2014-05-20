using System.Collections.Generic;
using System.Security.Principal;
using StackOverflowClone.Core;
using StackOverflowClone.Core.Indexes;
using StackOverflowClone.Models;

namespace StackOverflowClone.ViewModels
{
    public class HomeViewModel
    {
        public HomeViewModel(IPrincipal user)
        {
            User = new UserViewModel(user);
        }

        public List<Question> Questions { get; set; }
        public string Header { get; set; }
        public UserViewModel User { get; set; }
        public List<QuestionTagsIndex.ReduceResult> RecentlyUsedTags { get; set; }
        public List<QuestionTagsIndex.ReduceResult> MostUsedTags { get; set; }
    }
}