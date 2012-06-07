using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using HebMorph.CorpusReaders;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Similar;
using Lucene.Net.Search.Vectorhighlight;
using Lucene.Net.Store;
using LuceneNeatThings.ViewModels;
using SpellChecker.Net.Search.Spell;

namespace LuceneNeatThings.Core
{
	public static class Index
	{
		public static readonly IndexSearcher Searcher;
		public static byte PageSize { get; set; }

		static Index()
		{
			PageSize = 20;

			//Searcher = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(@"Z:\Projects\HebMorph\HebMorph.CorpusSearcher\HebMorph.CorpusSearcher\App_Data\Indexes\hewiki-new-20110520")), true);
			Searcher = new IndexSearcher(
				FSDirectory.Open(new DirectoryInfo(@"z:\Talks\LuceneNeatThings\Wikipedia\idx\"))
				, true);
		}

		/// <summary>
		/// Get terms starting with the given prefix
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		public static IEnumerable<string> GetTerms(string prefix, int maxItems = 10)
		{
			if (string.IsNullOrWhiteSpace(prefix))
				yield break;

			var counter = 0;
			var tagsEnum = new PrefixTermEnum(Searcher.GetIndexReader(),
				new Term("Title", prefix));
			while (tagsEnum.Next())
			{
				yield return tagsEnum.Term().Text();
				
				if (++counter == maxItems) yield break;
			}

			tagsEnum.Close();
		}

		/// <summary>
		/// Get terms starting with the given prefix
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		public static IEnumerable<string> GetTermsScored(string prefix, int maxItems = 10)
		{
			if (string.IsNullOrWhiteSpace(prefix))
				yield break;

			var results = Searcher.Search(new PrefixQuery(new Term("Title", prefix))
				, null, maxItems);

			foreach (var doc in results.ScoreDocs)
			{
				yield return Searcher.Doc(doc.doc).Get("Title");
			}
		}

		private static readonly string[] SearchFields = new[] { "Title", "Content" };
		private static readonly BooleanClause.Occur[] SearchFlags =
			new[] { BooleanClause.Occur.SHOULD, BooleanClause.Occur.SHOULD };

		public static SearchResultsViewModel Search(SearchQuery searchQuery)
		{
			var ret = new SearchResultsViewModel
			          	{
			          		SearchResults = new List<SearchResultsViewModel.SearchResult>(PageSize), Query = searchQuery
			          	};

			// Parse query, possibly throwing a ParseException
			Query query;
			if (searchQuery.TitlesOnly) // we only need to query on one field
			{
				var qp = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Title",
				                         new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
				query = qp.Parse(searchQuery.Query);
			}
			else // querying on both fields, Content and Title
			{
				query = MultiFieldQueryParser.Parse(Lucene.Net.Util.Version.LUCENE_29, searchQuery.Query,
													SearchFields, SearchFlags,
													new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29)
													);
			}

			// Init the highlighter instance
			var fvh = new FastVectorHighlighter(FastVectorHighlighter.DEFAULT_PHRASE_HIGHLIGHT,
												FastVectorHighlighter.DEFAULT_FIELD_MATCH,
												new SimpleFragListBuilder(),
												new ScoreOrderFragmentsBuilder(new[] { "[b]" }, new[] { "[/b]" })
												);

			// Perform the actual search
			var tsdc = TopScoreDocCollector.create(PageSize * searchQuery.CurrentPage, true);
			Searcher.Search(query, tsdc);
			ret.TotalResults = tsdc.GetTotalHits();
			var hits = tsdc.TopDocs().ScoreDocs;

			int i;
			for (i = (searchQuery.CurrentPage - 1) * PageSize; i < hits.Length; ++i)
			{
				var d = Searcher.Doc(hits[i].doc);
				var fq = fvh.GetFieldQuery(query);
				var fragment = fvh.GetBestFragment(fq, Searcher.GetIndexReader(), hits[i].doc, "Content", 400);

				ret.SearchResults.Add(new SearchResultsViewModel.SearchResult
				{
					Id = d.Get("Id"),
					Title = d.Get("Title"),
					Score = hits[i].score,
					LuceneDocId = hits[i].doc,
					Fragment = MvcHtmlString.Create(fragment.HtmlStripFragment()),
				});
			}
			return ret;
		}

		public static CorpusDocument GetDocument(string indexName, int indexDocumentId)
		{
			var doc = Searcher.Doc(indexDocumentId);
			var ret = new CorpusDocument
			{
				Id = doc.Get("Id"),
				Title = doc.Get("Title")
			};
			ret.SetContent(doc.Get("Content"), CorpusDocument.ContentFormat.Html);
			return ret;
		}

		public static SearchResultsViewModel SearchWithSuggestions(SearchQuery searchQuery, bool suggestOnlyWhenNoResults = false)
		{
			var ret = new SearchResultsViewModel
			          	{
			          		SearchResults = new List<SearchResultsViewModel.SearchResult>(PageSize), Query = searchQuery
			          	};

			// Parse query, possibly throwing a ParseException
			Query query;
			if (searchQuery.TitlesOnly)
			{
				var qp = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Title",
										 new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29)
										 );
				query = qp.Parse(searchQuery.Query);
			}
			else
			{
				query = MultiFieldQueryParser.Parse(Lucene.Net.Util.Version.LUCENE_29, searchQuery.Query,
													SearchFields, SearchFlags,
													new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29)
													);
			}

			// Perform the actual search
			var tsdc = TopScoreDocCollector.create(PageSize * searchQuery.CurrentPage, true);
			Searcher.Search(query, tsdc);
			ret.TotalResults = tsdc.GetTotalHits();
			var hits = tsdc.TopDocs().ScoreDocs;

			// Do the suggestion magic
			if (suggestOnlyWhenNoResults && ret.TotalResults == 0 || (!suggestOnlyWhenNoResults))
			{
				ret.Suggestions = new List<string>();
				var spellChecker = new SpellChecker.Net.Search.Spell.SpellChecker(Searcher.GetIndexReader().Directory());

				// This is kind of a hack to get things working quickly
				// for real-world usage we probably want to get the analyzed terms from the Query object
				var individualTerms = searchQuery.Query.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

				foreach (var term in individualTerms)
				{
					// we only specify field name if we actually got results,
					// to improve suggestions relevancy
					ret.Suggestions.AddRange(spellChecker.SuggestSimilar(term,
					                                            searchQuery.MaxSuggestions,
					                                            null,
					                                            ret.TotalResults == 0 ? null : "Title",
					                                            true));
				}
			}

			// Init the highlighter instance
			var fvh = new FastVectorHighlighter(FastVectorHighlighter.DEFAULT_PHRASE_HIGHLIGHT,
									FastVectorHighlighter.DEFAULT_FIELD_MATCH,
									new SimpleFragListBuilder(),
									new ScoreOrderFragmentsBuilder(new[] { "[b]" }, new[] { "[/b]" }));


			int i;
			for (i = (searchQuery.CurrentPage - 1) * PageSize; i < hits.Length; ++i)
			{
				var d = Searcher.Doc(hits[i].doc);
				var fq = fvh.GetFieldQuery(query);
				var fragment = fvh.GetBestFragment(fq, Searcher.GetIndexReader(),
					hits[i].doc, "Content", 400);

				ret.SearchResults.Add(new SearchResultsViewModel.SearchResult
				{
					Id = d.Get("Id"),
					Title = d.Get("Title"),
					Score = hits[i].score,
					LuceneDocId = hits[i].doc,
					Fragment = MvcHtmlString.Create(fragment.HtmlStripFragment()),
				});
			}
			return ret;
		}

		public static IList<CorpusDocument> GetMoreLikeThis(string indexName, int indexDocumentId, int maxDocs)
		{
			// See: http://lucene.apache.org/java/2_2_0/api/org/apache/lucene/search/similar/MoreLikeThis.html

			var mlt = new MoreLikeThis(Searcher.GetIndexReader());
			mlt.SetAnalyzer(new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
			mlt.SetFieldNames(new[] { "Title", "Content" });
			mlt.SetMinWordLen(4); // improve relevancy

			var query = mlt.Like(indexDocumentId);

			var tsdc = TopScoreDocCollector.create(maxDocs, true);
			Searcher.Search(query, tsdc);
			var hits = tsdc.TopDocs().ScoreDocs;

			var ret = new List<CorpusDocument>(maxDocs);

			foreach (var hit in hits)
			{
				var d = Searcher.Doc(hit.doc);
				ret.Add(new CorpusDocument
				{
					Id = d.Get("Id"),
					Title = d.Get("Title"),
				});
			}
			return ret;
		}
	}
}