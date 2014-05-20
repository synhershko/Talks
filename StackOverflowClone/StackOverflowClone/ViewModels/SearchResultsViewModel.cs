using System.Collections.Generic;
using System.Security.Principal;
using StackOverflowClone.Core;

namespace StackOverflowClone.ViewModels
{
    public class SearchResultsViewModel
    {
        public SearchResultsViewModel(IPrincipal user)
        {
            User = new UserViewModel(user);
        }

        public UserViewModel User { get; set; }
        public List<QuestionLightViewModel> Questions { get; set; }
        public int ResultsCount { get; set; }
        public string Header { get; set; }
    }
}