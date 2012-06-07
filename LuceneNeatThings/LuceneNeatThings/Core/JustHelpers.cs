using System.Web.Mvc;
using HebMorph.CorpusReaders;
using LuceneNeatThings.ViewModels;

namespace LuceneNeatThings.Core
{
	public static class JustHelpers
	{
		public static Document ToViewDocument(this CorpusDocument doc)
		{
			return new Document
			       	{
			       		Content = MvcHtmlString.Create(doc.Content),
			       		Id = doc.Id,
			       		Title = MvcHtmlString.Create(doc.Title)
			       	};
		}
	}
}