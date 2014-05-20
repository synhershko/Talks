using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using StackOverflowClone.Models;

namespace StackOverflowClone.Core
{
    /// <summary>
    /// This class is used to make sure all Models have been properly set up
    /// </summary>
    public class CompilationBlocker
    {
        public CompilationBlocker()
        {
            var q = new Question()
            {
                Answers = new Collection<Answer>(),
                Comments = new Collection<Comment>(),
                Content = string.Empty,
                CreatedBy = string.Empty,
                CreatedOn = DateTimeOffset.UtcNow,
                Id = string.Empty,
                Stats = new Stats(),
                Subject = string.Empty,
                Tags = new Collection<string>(),
            };

            var a = new Answer()
            {
                Comments = new Collection<Comment>(),
                Content = string.Empty,
                CreatedByUserId = string.Empty,
                CreatedOn = DateTimeOffset.UtcNow,
                Stats = new Stats(),
                LastEditedOn = (DateTimeOffset?) null,
            };

            var c = new Comment()
            {
                Content = string.Empty,
                CreatedOn = DateTimeOffset.UtcNow,
                CreatedByUserId = string.Empty,
                UpVoteCount = 0,
            };

            var s = new Stats()
            {
                DownVoteCount = 0,
                UpVoteCount = 0,
                FavoriteCount = 0,
                ViewsCount = 0,
            };

            var u = new User()
            {
                CreatedOn = DateTime.UtcNow,
                DisplayName = string.Empty,
                Email = string.Empty,
                FullName = string.Empty,
                FavoriteTags = new List<string>(),
                Id = string.Empty,
                IsActive = true,
                Reputation = 0,
            };
        }
    }
}