using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Lucene.Net.QueryParsers;
using LuceneNeatThings.Core;
using LuceneNeatThings.ViewModels;

namespace LuceneNeatThings.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			ViewBag.Query = new SearchQuery();

			return View();
		}

		public ActionResult AutoComplete()
		{
			ViewBag.Query = new SearchQuery();

			return View();
		}

		public ActionResult AutoCompleteJson(string prefix)
		{
			return Json(Core.Index.GetTermsScored(prefix).ToList(), JsonRequestBehavior.AllowGet);
		}

		public ActionResult Search(string query, int? page, string titlesonly)
		{
			var pageNumber = page ?? 1;
			if (pageNumber <= 0)
				pageNumber = 1;

			var q = new SearchQuery
			{
				Query = query,
				CurrentPage = pageNumber,
				TitlesOnly = !string.IsNullOrWhiteSpace(titlesonly),
			};

			var vm = new SearchResultsViewModel {Query = q, SearchResults = new List<SearchResultsViewModel.SearchResult>(), TotalResults = 0};
			try
			{
				vm = Core.Index.SearchWithSuggestions(q);
			}
			catch (ParseException ex)
			{
				ViewBag.ErrorMessage = "Error: " + ex.Message;
			}

			ViewBag.Query = q;

			return View(vm);
		}

		public ActionResult ViewDocument(string corpusName, int indexDocId)
		{
			var doc = Core.Index.GetDocument(corpusName, indexDocId);
			return View(doc.ToViewDocument());
		}

		public ActionResult MoreLikeThis(string corpusName, int indexDocId)
		{
			var docs = Core.Index.GetMoreLikeThis(corpusName, indexDocId, 10);
			return View(docs.Select(x => x.ToViewDocument()));
		}
	}
}
