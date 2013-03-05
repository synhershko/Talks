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
                                              CreatedBy = user.DisplayName,
                                              q.CreatedOn,
                                              q.Tags,
                                              AnswersCount = q.Answers.Count,
                                              TotalVotes = stats.UpVoteCount - stats.DownVoteCount,
                                              stats.ViewsCount,
                                              stats.FavoriteCount,
                                              UserReputation = user.Reputation,
                                              ForSearch = new object[]
                                                              {
                                                                  q.Tags.Select(tag => tag),
                                                                  q.Subject,
                                                                  q.Content,
                                                                  q.Answers.Select(a => a.Content),
                                                                  user.DisplayName,
                                                              }
                                          };

            Store("AnswersCount", FieldStorage.Yes);
            Store("TotalVotes", FieldStorage.Yes);
            Store("ViewsCount", FieldStorage.Yes);
            Store("FavoriteCount", FieldStorage.Yes);
            Store("CreatedBy", FieldStorage.Yes);
            Store("UserReputation", FieldStorage.Yes);
            
            Index("ForSearch", FieldIndexing.Analyzed); // Defaults to an English standard analyzer
        }
    }
}