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
            var header = "Top Questions";

            // All questions, ordered by most recent.
            var questionsQuery = RavenSession.Query<Question>()
                .OrderByDescending(x => x.CreatedOn)
                .Take(20);

            // Filter Questions by Tags?
            if (!string.IsNullOrEmpty(tag))
            {
                header = "Questions Tagged '" + tag + "'";
                questionsQuery = questionsQuery.Where(x => x.Tags.Any(y => y == tag));
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
            viewModel.Header = header;
            viewModel.User = new UserViewModel(User) { Id = User.Identity.Name, Name = User.Identity.Name };
            viewModel.Questions = questionsQuery.AsProjection<QuestionLightViewModel>().ToList();
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