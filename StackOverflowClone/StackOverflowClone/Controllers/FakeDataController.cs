using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;
using StackOverflowClone.Core;
using StackOverflowClone.Models;

namespace StackOverflowClone.Controllers
{
    public class FakeDataController : RavenController
    {
        [HttpGet]
        public ActionResult Create()
        {
            var users = CreateFakeUsers();
            foreach (var user in users)
            {
                RavenSession.Store(user);
            }

            var questions = CreateFakeQuestions(users);
            foreach (var question in questions)
            {
                RavenSession.Store(question);
                RavenSession.Store(question.Stats, question.Id + "/stats");
            }

            return Json(new { Done = "yes" });
        }

        private static readonly IList<string> FakeTags = new List<string>
                                                             {
                                                                 "ravendb",
                                                                 "asp.net-mvc",
                                                                 "c#",
                                                                 "linq",
                                                                 "moq",
                                                                 ".net",
                                                                 ".net3.5",
                                                                 "elmah",
                                                                 "yui-compressor",
                                                                 "minify",
                                                                 "mono",
                                                                 "asp.net-mvc3",
                                                                 "ruby-on-rails",
                                                                 "elmah",
                                                                 "rubygems"
                                                             };

        private static IEnumerable<User> CreateFixedFakeUsers()
        {
            return new List<User>
                       {
                           new User
                               {
                                   FullName = "Itamar Syn-Hershko",
                                   DisplayName = "synhershko",
                                   Email = "itamar@example.com",
                                   CreatedOn = new DateTime(2010, 5, 23, 13, 05, 00),
                                   Reputation = 69,
                                   IsActive = true,
                                   FavoriteTags = new List<string> {"ravendb", "c#", "asp.net-mvc3"}
                               }
                       };
        }

        public IList<User> CreateFakeUsers(int? numberOfFakeUsers = null)
        {
            if (numberOfFakeUsers == null)
                numberOfFakeUsers = GetRandom.Int(20, 100);

            var fakeUsers = new List<User>(CreateFixedFakeUsers());
            for (int i = 0; i < numberOfFakeUsers; i++)
            {
                fakeUsers.Add(CreateAFakeUser());
            }

            return fakeUsers;
        }

        private static IEnumerable<Question> CreateFakeQuestions(IList<User> users, int? numberOfFakeQuestions = null)
        {
            if (users == null)
            {
                throw new ArgumentException("Users list cannot be null");
            }

            if (numberOfFakeQuestions == null)
                numberOfFakeQuestions = GetRandom.Int(20, 100);

            var fakeQuestions = new List<Question>();
            for (int i = 0; i < numberOfFakeQuestions; i++)
            {
                fakeQuestions.Add(CreateAFakeQuestion(users));
            }

            return fakeQuestions;
        }

        private static Question CreateAFakeQuestion(IList<User> users)
        {
            var user = users.ToRandomList(1).Single();
            var fakeQuestion = Builder<Question>
                .CreateNew()
                .With(x => x.Id = null)
                .With(x => x.Subject = GetRandom.Phrase(GetRandom.Int(10, 50)))
                .With(x => x.Content = GetRandom.Phrase(GetRandom.Int(30, 500)))
                .And(x => x.CreatedBy = user.Id)
                .And(x => x.CreatedOn = GetRandom.DateTime(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMinutes(-5)))
                .And(x => x.Tags = FakeTags.ToRandomList(GetRandom.Int(1, 5)))
                .And(x => x.Stats = CreateAFakeStats())
                .Build();

            for (int i = 0; i < (GetRandom.Int(1, 20) <= 10 ? GetRandom.PositiveInt(6) : 0); i++)
            {
                if (fakeQuestion.Answers == null)
                {
                    fakeQuestion.Answers = new List<Answer>();
                }

                fakeQuestion.Answers.Add(CreateAFakeAnswer(fakeQuestion.CreatedOn, users));
            }

            if (fakeQuestion.Answers == null)
            {
                fakeQuestion.Answers = new List<Answer>();
            }

            return fakeQuestion;
        }

        private static Stats CreateAFakeStats()
        {
            return GetRandom.PositiveInt(5) == 1
                       ? new Stats()
                       : Builder<Stats>
                             .CreateNew()
                             .With(y => y.UpVoteCount = GetRandom.PositiveInt(20))
                             .And(y => y.DownVoteCount = GetRandom.PositiveInt(3))
                             .And(y => y.FavoriteCount = GetRandom.PositiveInt(10))
                             .And(y => y.ViewsCount == GetRandom.PositiveInt(10000))
                             .Build();
        }

        private static Answer CreateAFakeAnswer(DateTimeOffset questionCreatedOn, IList<User> users)
        {
            var userId = users.ToRandomList(1).Single().Id;
            var answer = Builder<Answer>
                .CreateNew()
                .With(x => x.Content = GetRandom.Phrase(GetRandom.Int(20, 100)))
                .And(x => x.CreatedOn = GetRandom.DateTime(questionCreatedOn.DateTime.AddMinutes(5), DateTime.UtcNow))
                .And(x => x.CreatedByUserId = userId)
                .And(x => x.Stats = CreateAFakeStats())
                .Build();

            for (int i = 0; i < (GetRandom.PositiveInt(20) <= 10 ? GetRandom.PositiveInt(6) : 0); i++)
            {
                if (answer.Comments == null)
                {
                    answer.Comments = new List<Comment>();
                }
                answer.Comments.Add(CreateAFakeComment(questionCreatedOn.DateTime, users.ToRandomList(1).Single().Id));
            }

            if (answer.Comments == null)
            {
                answer.Comments = new List<Comment>();
            }

            return answer;
        }

        private static Comment CreateAFakeComment(DateTime questionCreatedOn, string userId)
        {
            return Builder<Comment>
                .CreateNew()
                .With(x => x.Content = GetRandom.Phrase(GetRandom.PositiveInt(20)))
                .And(x => x.CreatedOn = GetRandom.DateTime(questionCreatedOn.AddMinutes(5), DateTime.UtcNow))
                .And(x => x.CreatedByUserId = userId)
                // If Rand number between 1-10 <= 5, then votes = another rand numb between 1-5. else 0.
                .And(x => x.UpVoteCount = (GetRandom.PositiveInt(10) <= 6 ? GetRandom.PositiveInt(5) : 0))
                .Build();
        }

        private static User CreateAFakeUser()
        {
            return Builder<User>
                .CreateNew()
                .With(x => x.Id = null)
                .With(x => x.FullName = string.Format("{0} {1}", GetRandom.FirstName(), GetRandom.LastName()))
                .And(x => x.DisplayName = x.FullName.Replace(' ', '.'))
                .And(x => x.Email = GetRandom.Email())
                .And(x => x.CreatedOn = GetRandom.DateTime(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow))
                .And(x => x.Reputation = GetRandom.PositiveInt(50000))
                .And(x => x.IsActive = GetRandom.Boolean())
                .Build();
        }
    }
}