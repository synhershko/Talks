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
                               select new
                                          {
                                              q.CreatedOn,
                                              q.CreatedBy,
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
        }
    }
}