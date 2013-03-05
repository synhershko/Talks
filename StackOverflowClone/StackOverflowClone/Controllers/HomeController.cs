using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using Raven.Client;
using Raven.Client.Linq;
using StackOverflowClone.Core;
using StackOverflowClone.Core.Indexes;
using StackOverflowClone.Models;

namespace StackOverflowClone.Controllers
{
    public class HomeController : RavenController
    {
        [HttpGet]
        public ActionResult Index(string tag)
        {
            var questions = RavenSession.Query<Question>();
            var header = "Top Questions";

            if (!string.IsNullOrWhiteSpace(tag))
            {
                header = "Questions Tagged '" + tag + "'";
                questions = questions.Where(x => x.Tags.Any(y => y.Equals(tag)));
            }
            else
            {
                questions = questions.OrderByDescending(x => x.CreatedOn);
            }


            var mostUsedTags = RavenSession.Query<QuestionTagsIndex.ReduceResult, QuestionTagsIndex>()
                       .OrderByDescending(x => x.Count)
                        .Take(20)
                        .ToList();

            var recentlyUsedTags = RavenSession.Query<QuestionTagsIndex.ReduceResult, QuestionTagsIndex>()
                        .OrderByDescending(x => x.LastUsed)
                        .Take(20)
                        .ToList();
                
            dynamic viewModel = new ExpandoObject();
            viewModel.Questions = questions.Take(20).AsProjection<QuestionLightViewModel>().ToList(); ;
            viewModel.Header = header;
            viewModel.User = new UserViewModel(User) {Id = User.Identity.Name, Name = User.Identity.Name};
            viewModel.RecentlyUsedTags = recentlyUsedTags;
            viewModel.MostUsedTags = mostUsedTags;
            return View(viewModel);
        }

        public ActionResult SignOut()
        {
            throw new System.NotImplementedException();
        }

        public ActionResult Authenticate(string returnurl)
        {
            throw new System.NotImplementedException();
        }
    }
}