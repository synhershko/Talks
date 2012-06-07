using System.Text;
using System.Web.Mvc;
using HebMorph.CorpusReaders;

namespace LuceneNeatThings.Core
{
	public static class FormattingHelpers
	{
		//private static MarkdownSharp.Markdown MarkdownConverter
		//{
		//    get { return _markdownConverter ?? (_markdownConverter = new MarkdownSharp.Markdown()); }
		//}
		//private static MarkdownSharp.Markdown _markdownConverter;

		public static void Foo(CorpusDocument doc)
		{
			string redirectToTopic;
			var htmlContent = ScrewTurn.Wiki.Formatter.Format(doc.Title, doc.Content, new { Name = doc.Title, TopicId = doc.Id },
															  true, false, out redirectToTopic);
		}

		public static string AsHtml(this CorpusDocument doc)
		{
			switch (doc.Format)
			{
				//case CorpusDocument.ContentFormat.Markdown:
				//    return MarkdownConverter.Transform(doc.Content);
				case CorpusDocument.ContentFormat.WikiMarkup:
					string redirectToTopic;
					var htmlContent = ScrewTurn.Wiki.Formatter.Format(doc.Title, doc.Content,
																	  new
																		{
																			Name = doc.Title,
																			TopicId = doc.Id
																		},
																	  true, false,
																	  out redirectToTopic);

					//string redirectToTopic;
					//var htmlContent = ScrewTurn.Wiki.Formatter.Format(doc.Title, doc.Content, new {Name = doc.Title, TopicId = doc.Id},
					//                                                  true, false, out redirectToTopic);

					// we currently do not support the notion of redirects
					if (htmlContent.StartsWith("Redirected to") || htmlContent.StartsWith("<ol><li>הפניה"))
						return string.Empty;

					// make up for dumb <br> handling by the formatter
					int loc = 0, tmp = 0;
					var sb = new StringBuilder(htmlContent.Length);
					while ((tmp = htmlContent.IndexOf("<br /><br />", loc, System.StringComparison.Ordinal)) > 0)
					{
						sb.Append(htmlContent.Substring(loc, tmp - loc));
						sb.Append("<br />");
						tmp += "<br /><br />".Length;
						while (tmp + "<br />".Length < htmlContent.Length && "<br />".Equals(htmlContent.Substring(tmp, "<br />".Length)))
							tmp += "<br />".Length;
						loc = tmp;
					}
					sb.Append(htmlContent.Substring(loc, htmlContent.Length - loc));

					return sb.ToString().Trim();
			}
			return doc.Content; // either a fallback or it is already HTML
		}

		public static string HtmlStripFragment(this string fragment)
		{
			if (string.IsNullOrEmpty(fragment)) return string.Empty;

			var sb = new StringBuilder(fragment.Length);
			bool withinHtml = false, first = true;
			foreach (var c in fragment)
			{
				if (c == '>')
				{
					if (first) sb.Length = 0;
					withinHtml = false;
					first = false;
					continue;
				}
				if (withinHtml)
					continue;
				if (c == '<')
				{
					first = false;
					withinHtml = true;
					continue;
				}
				sb.Append(c);
			}

			return sb.Append("...").Replace("[b]", "<b>").Replace("[/b]", "</b>").ToString();
		}
	}
}