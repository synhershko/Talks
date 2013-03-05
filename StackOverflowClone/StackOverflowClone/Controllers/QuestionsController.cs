using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Bundles.MoreLikeThis;
using System.Web.Mvc;
using StackOverflowClone.Core;
using StackOverflowClone.Core.Indexes;
using StackOverflowClone.Models;

namespace StackOverflowClone.Controllers
{
    public class QuestionsController : RavenController
    {
        public ActionResult View(int id)
        {
            var q = RavenSession.Include<Question>(x => x.CreatedBy)
                .Include("Comments,CreatedByUserId")
                .Include("Answers,CreatedByUserId")
                .Load(id);

            if (q == null)
                return HttpNotFound();

            q.Stats = RavenSession.Load<Stats>(q.Id + "/stats");
            q.Stats.ViewsCount++;

            // Since we are using Includes, this entire code block will not access the server even once
            var users = new Dictionary<string, User>();
            users.Add(q.CreatedBy, RavenSession.Load<User>(q.CreatedBy));
            foreach (var answer in q.Answers)
            {
                users.Add(answer.CreatedByUserId, RavenSession.Load<User>(answer.CreatedByUserId));
            }
            if (q.Comments != null)
            {
                foreach (var comment in q.Comments)
                {
                    users.Add(comment.CreatedByUserId, RavenSession.Load<User>(comment.CreatedByUserId));
                }
            }

            dynamic viewModel = new ExpandoObject();
            viewModel.User = new UserViewModel(User) {Id = User.Identity.Name, Name = User.Identity.Name};
            viewModel.Question = q;
            viewModel.Users = users;
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Ask()
        {
            dynamic viewModel = new ExpandoObject();
            viewModel.User = new UserViewModel(User) { Id = User.Identity.Name, Name = User.Identity.Name };
            viewModel.Question = new QuestionInputModel();
            return View(viewModel);
        }

        [HttpPost] // Authorize
        public ActionResult Ask(QuestionInputModel inputModel)
        {
            if (ModelState.IsValid)
            {
                var q = inputModel.ToQuestion();
                q.CreatedBy = "users/1"; // Just a stupid default because we haven't implemented log-in

                RavenSession.Store(q);
                RavenSession.Store(new Stats(), q.Id + "/stats");

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            dynamic viewModel = new ExpandoObject();
            viewModel.User = new UserViewModel(User) { Id = User.Identity.Name, Name = User.Identity.Name };
            viewModel.Question = inputModel;
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Answer(int id, AnswerInputModel input)
        {
            var q = RavenSession.Load<Question>(id);
            if (q == null)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                q.Answers.Add(new Answer
                                  {
                                      Comments = new List<Comment>(),
                                      Content = input.Content,
                                      CreatedByUserId = "users/1",  // again, just a stupid default
                                      CreatedOn = DateTimeOffset.UtcNow,
                                      Stats = new Stats(),
                                  });
            }

            return RedirectToAction("View", new {id = id});
        }

        public ActionResult Search(string q)
        {
            var questionsQuery = RavenSession.Advanced.LuceneQuery<Question, QuestionsIndex>()
                                             .Search("ForSearch", q);

            RavenQueryStatistics stats;

            dynamic viewModel = new ExpandoObject();
            viewModel.User = new UserViewModel(User) {Id = User.Identity.Name, Name = User.Identity.Name};
            viewModel.Questions = questionsQuery.SelectFields<QuestionLightViewModel>().Statistics(out stats).ToList();
            viewModel.ResultsCount = stats.TotalResults;
            viewModel.Header = stats.TotalResults + " results for " + q;

            return View("List", viewModel);
        }
    }
}
