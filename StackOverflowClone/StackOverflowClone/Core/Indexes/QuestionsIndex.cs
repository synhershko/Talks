using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using StackOverflowClone.Models;

namespace StackOverflowClone.Core.Indexes
{
    public class QuestionsIndex : AbstractIndexCreationTask<Question>
    {
        public QuestionsIndex()
        {
            Map = questions => from q in questions
                               let stats = LoadDocument<Stats>(q.Id + "/stats")
                               let user = LoadDocument<User>(q.CreatedBy)
                               select new
                                          {
                                              q.CreatedOn,
                                              CreatedBy = user.DisplayName,
                                              UserReputation = user.Reputation,
                                              AnswersCount = q.Answers.Count,
                                              q.Tags,
                                              stats.ViewsCount,
                                              TotalVotes = stats.UpVoteCount - stats.DownVoteCount,
                                              stats.FavoriteCount
                                          };

            Store("AnswersCount", FieldStorage.Yes);
            Store("ViewsCount", FieldStorage.Yes);
            Store("FavouriteCount", FieldStorage.Yes);
            Store("TotalVotes", FieldStorage.Yes);
            Store("CreatedBy", FieldStorage.Yes);
            Store("UserReputation", FieldStorage.Yes);
        }
    }
}