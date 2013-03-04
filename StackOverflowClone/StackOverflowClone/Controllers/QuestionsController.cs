using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Web.Mvc;
using StackOverflowClone.Core;
using StackOverflowClone.Models;

namespace StackOverflowClone.Controllers
{
    public class QuestionsController : RavenController
    {
        public ActionResult View(int id)
        {
            var question = RavenSession.Load<Question>(id);
            if (question == null)
            {
                return new HttpNotFoundResult();
            }
            
            question.Stats = RavenSession.Load<Stats>(question.Id + "/stats");
            question.Stats.ViewsCount++;

            dynamic viewModel = new ExpandoObject();
            viewModel.User = new UserViewModel(User) { Id = User.Identity.Name, Name = User.Identity.Name };
            viewModel.Question = question;

            return View("View", viewModel);
        }

        [HttpGet]
        public ActionResult Ask()
        {
            dynamic viewModel = new ExpandoObject();
            viewModel.User = new UserViewModel(User) { Id = User.Identity.Name, Name = User.Identity.Name };
            viewModel.Question = new QuestionInputModel();

            return View("Ask", viewModel);
        }

        [HttpPost] // Authorize
        public ActionResult Ask(QuestionInputModel inputModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var question = inputModel.ToQuestion();
                    question.CreatedBy = "users/1"; // Just a stupid default because we haven't implemented log-in

                    RavenSession.Store(question);
                    RavenSession.Store(new Stats(), question.Id + "/stats");

                    return RedirectToAction("Index", "Home", new { area = "" });
                }
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("Error", exception.Message);
            }

            dynamic viewModel = new ExpandoObject();
            viewModel.User = new UserViewModel(User) { Id = User.Identity.Name, Name = User.Identity.Name };
            viewModel.Question = inputModel;

            return View("Ask", viewModel);
        }

        [HttpPost]
        public ActionResult Answer(int id, AnswerInputModel input)
        {
            var question = RavenSession.Load<Question>(id);
            if (question == null)
            {
                return new HttpNotFoundResult();
            }

            if (ModelState.IsValid)
            {
                question.Answers.Add(new Answer
                                         {
                                             Comments = new List<Comment>(),
                                             Content = input.Content,
                                             CreatedByUserId = "users/1", // again, just a stupid default
                                             CreatedOn = DateTimeOffset.UtcNow,
                                             Stats = new Stats()
                                         });
            }
            
            return RedirectToAction("View", new {id = id});
        }
    }
}
