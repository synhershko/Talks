using Lucene.Net.QueryParsers;
using LuceneNeatThings.Core;

namespace LuceneNeatThings.ViewModels
{
	public class SearchQuery
	{
		public SearchQuery()
		{
			MaxSuggestions = 5;
		}

		public string Query { get; set; }
		public QueryParser.Operator DefaultOperator { get; set; }
		public string IndexName { get; set; }

		public bool TitlesOnly { get; set; }
		public int MaxSuggestions { get; set; }

		public int CurrentPage { get; set; }

		public string GetSearchUrl()
		{
			return string.Format("/Search?corpusName={0}&query={1}&page=", IndexName, Query.UrlEncode());
		}
	}
}