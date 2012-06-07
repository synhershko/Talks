using System.Web.Mvc;

namespace LuceneNeatThings.ViewModels
{
	public class Document
	{
		public string Id { get; set; }
		public MvcHtmlString Title { get; set; }
		public MvcHtmlString Content { get; set; }
	}
}