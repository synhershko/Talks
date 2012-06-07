using System.Collections.Generic;
using System.Web.Mvc;

namespace LuceneNeatThings.ViewModels
{
	public class SearchResultsViewModel
	{
		public class SearchResult
		{
			public string Id { get; set; }
			public string Title { get; set; }
			public int LuceneDocId { get; set; }
			public float Score { get; set; }
			public MvcHtmlString Fragment { get; set; }
		}

		public int TotalResults { get; set; }
		public List<SearchResult> SearchResults { get; set; }
		public List<string> Suggestions { get; set; }
		public SearchQuery Query { get; set; }
	}
}