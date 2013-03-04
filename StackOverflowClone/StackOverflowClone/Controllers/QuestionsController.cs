using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Bundles.MoreLikeThis;
using System.Web.Mvc;
using StackOverflowClone.Core;
using StackOverflowClone.Models;

namespace StackOverflowClone.Controllers
{
    public class QuestionsController : Controller
    {
        public ActionResult View(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public ActionResult Ask()
        {
            throw new NotImplementedException();
        }

        [HttpPost] // Authorize
        public ActionResult Ask(QuestionInputModel inputModel)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public ActionResult Answer(int id, AnswerInputModel input)
        {
            throw new NotImplementedException();
        }
    }
}
