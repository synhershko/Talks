using System;
using System.Linq;
using Raven.Client.Indexes;
using StackOverflowClone.Models;

namespace StackOverflowClone.Core.Indexes
{
    public class QuestionTagsIndex : AbstractIndexCreationTask<Question, QuestionTagsIndex.ReduceResult>
    {
        public class ReduceResult
        {
            public string Tag { get; set; }
            public int Count { get; set; }
            public DateTimeOffset LastUsed { get; set; }
        }


        public QuestionTagsIndex()
        {
            Map = questions => from q in questions
                               from tag in q.Tags
                               select new
                                          {
                                              Tag = tag,
                                              Count = 1,
                                              LastUsed = q.CreatedOn,
                                          };

            Reduce = results => from r in results
                                group r by r.Tag
                                into g
                                select new
                                           {
                                               Tag = g.Key,
                                               Count = g.Sum(x => x.Count),
                                               LastUsed = g.Max(x => x.LastUsed)
                                           };
        }
    }
}