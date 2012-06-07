/* This file is taken from ScrewTurn C# wiki: http://www.screwturn.eu/
 * Permission was granted from the authors to be used in a non-GPLv2 compatible
 * project as long as this header is intact.
 * 
 * Note this is an old version of the formatter, and has went through several
 * breaking changes to make it work for He-Wikipedia and HebMorph.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ScrewTurn.Wiki
{
	/// <summary>
	/// Performs all the text formatting and parsing operations.
	/// </summary>
	public static class Formatter
	{

		private static Regex noWiki = new Regex(@"\<nowiki\>(.|\n|\r)+?\<\/nowiki\>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		private static Regex link = new Regex(@"(\[\[.+?\]\])|(\[.+?\])", RegexOptions.Compiled);
		private static Regex redirection = new Regex(@"^\#REDIRECT.*\[\[(.*)\]\]", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
		private static Regex h1 = new Regex(@"^==.+?==", RegexOptions.Compiled | RegexOptions.Multiline);
		private static Regex h2 = new Regex(@"^===.+?===", RegexOptions.Compiled | RegexOptions.Multiline);
		private static Regex h3 = new Regex(@"^====.+?====", RegexOptions.Compiled | RegexOptions.Multiline);
		private static Regex h4 = new Regex(@"^=====.+?=====", RegexOptions.Compiled | RegexOptions.Multiline);
		private static Regex bold = new Regex(@"'''.+?'''", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex italic = new Regex(@"''.+?''", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex boldItalic = new Regex(@"'''''.+?'''''", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex underlined = new Regex(@"__.+?__", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex striked = new Regex(@"(?<!\<\!)(\-\-(?!\>).+?\-\-)(?!\>)", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex code = new Regex(@"\{\{.+?\}\}", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex pre = new Regex(@"\{\{\{\{.+?\}\}\}\}", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex box = new Regex(@"\(\(\(.+?\)\)\)", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex specialTag = new Regex(@"\{(wikititle|wikiversion|mainurl|up|rsspage|themepath|clear|br|top|searchbox|ftsearchbox|pagecount|cloud)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		private static Regex list = new Regex(@"(?<=(\n|^))((\*|\#)+(\ )?.+?\n)+", RegexOptions.Compiled);
		private static Regex toc = new Regex(@"\{toc\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		private static Regex transclusion = new Regex(@"\{T(\:|\|).+}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		private static Regex hr = new Regex(@"(?<=(\n|^))(\ )*----(\ )*\n", RegexOptions.Compiled);
		private static Regex snippet = new Regex(@"\{S(\:|\|)(.+?)(\|(.+?))*}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		private static Regex table = new Regex(@"\{\|(\ [^\n]*)?\n.+?\|\}", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex indent = new Regex(@"(?<=(\n|^))\:+(\ )?.+?\n", RegexOptions.Compiled);
		private static Regex esc = new Regex(@"\<esc\>(.|\n|\r)*?\<\/esc\>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		private static Regex sign = new Regex(@"§§\(.+?\)§§", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static Regex fullCode = new Regex(@"@@.+?@@", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex username = new Regex(@"\{username\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		private static Regex javascript = new Regex(@"\<script.*?\>.*?\<\/script\>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
		private static Regex math = new Regex(@"\<math\>(.|\n|\r)+?\<\/math\>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

		private static Regex lang_link = new Regex(@"([a-z\-]{2,}):\s*(.+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static string tocTitlePlaceHolder = String.Empty;
		private static string upReplacement = "GetFile.aspx?File=";

		private const string WikiTitle = "Wikipedia";
		private const string WikiVersion = "1.0";
		private const string MainUrl = "http://localhost/";
		//private static string PageExtension = String.Empty;

		/// <summary>
		/// Formats WikiMarkup, converting it into XHTML.
		/// </summary>
		/// <param name="documentName">The name of the document</param>
		/// <param name="raw">The raw WikiMarkup text.</param>
		/// <param name="current">The current Page (can be null).</param>
		/// <param name="is_rtl">Should the XHTML output have right-to-left text flow (e.g. for Hebrew)</param>
		/// <returns>The formatted text.</returns>
		public static string Format(string documentName, string raw, dynamic current, bool is_rtl, bool surroundWithBody, out string redirectToTopic)
		{
			var sb = new StringBuilder(raw);
			string tmp, a, n;
			StringBuilder dummy; // Used for temporary string manipulation inside formatting cycles
			var done = false;
			List<int> noWikiBegin = new List<int>(), noWikiEnd = new List<int>();
			int end = 0;
			var hPos = new List<HPosition>();

			sb.Replace("\r", "");
			if (!sb.ToString().EndsWith("\n")) sb.Append("\n"); // Very important to make Regular Expressions work!

			// Remove all double-LF in JavaScript tags
			var match = javascript.Match(sb.ToString());
			while (match.Success)
			{
				sb.Remove(match.Index, match.Length);
				sb.Insert(match.Index, match.Value.Replace("\n\n", "\n"));
				match = javascript.Match(sb.ToString(), match.Index + 1);
			}

			ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);

			redirectToTopic = null;

			if (current != null)
			{
				// Check redirection
				match = redirection.Match(sb.ToString());

				if (match.Success)
				{
					redirectToTopic = match.Groups[1].Value;
					return "Redirected to " + redirectToTopic;
				}
			}

			// Before Producing HTML
			match = fullCode.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					string content = match.Value.Substring(2, match.Length - 4);
					dummy = new StringBuilder();
					dummy.Append("<pre>");
					// IE needs \r\n for line breaks
					dummy.Append(EscapeWikiMarkup(content).Replace("\n", "\r\n"));
					dummy.Append("</pre>");
					sb.Insert(match.Index, dummy.ToString());
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = fullCode.Match(sb.ToString(), end);
			}

			// No more needed (Striked Regex modified)
			// Temporarily "escape" comments
			//sb.Replace("<!--", "($_^)");
			//sb.Replace("-->", "(^_$)");

			ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);

			match = math.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					// TODO ?
					/*
					sb.Insert(match.Index,
						String.Format("<img src=\"{0}\" />",
							WebServer.Instance.GenerateTeXUrl(match.Value.Substring(6, match.Value.Length - 13))));
					 */
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = math.Match(sb.ToString(), end);
			}

			// Before Producing HTML
			match = esc.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					sb.Insert(match.Index, match.Value.Substring(5, match.Length - 11).Replace("<", "&lt;").Replace(">", "&gt;"));
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = esc.Match(sb.ToString(), end);
			}

			// Snippets were here
			// Moved to solve problems with lists and tables

			match = table.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					sb.Insert(match.Index, BuildTable(match.Value));
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = table.Match(sb.ToString(), end);
			}

			match = indent.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					sb.Insert(match.Index, BuildIndent(match.Value) + "\n");
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = indent.Match(sb.ToString(), end);
			}

			match = specialTag.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					switch (match.Value.Substring(1, match.Value.Length - 2).ToUpperInvariant())
					{
						case "WIKITITLE":
							sb.Insert(match.Index, WikiTitle);
							break;
						case "WIKIVERSION":
							sb.Insert(match.Index, WikiVersion);
							break;
						case "MAINURL":
							sb.Insert(match.Index, MainUrl);
							break;
						case "UP":
							sb.Insert(match.Index, upReplacement);
							break;
						/*
						case "RSSPAGE":
							if(current != null) {
								sb.Insert(match.Index, @"<a href=""RSS.aspx?Page=" + Tools.UrlEncode(current.Name) + @""" title=""" + Exchanger.ResourceExchanger.GetResource("RssForThisPage") + @"""><img src=""" + Settings.ThemePath + @"Images/RSS.png"" alt=""RSS"" /></a>");
							}
							break;
						case "THEMEPATH":
							sb.Insert(match.Index, Settings.ThemePath);
							break; */
						case "CLEAR":
							sb.Insert(match.Index, @"<div style=""clear: both;""></div>");
							break;
						case "BR":
							sb.Insert(match.Index, "<br />");
							break;
						case "TOP":
							sb.Insert(match.Index, @"<a href=""#PageTop"">Top</a>");
							break;
						/*
						case "SEARCHBOX":
							sb.Insert(match.Index, @"<nowiki><input type=""text"" id=""TxtSearchBox"" onkeydown=""javascript:var keycode; if(window.event) keycode = event.keyCode; else keycode = event.which; if(keycode == 10 || keycode == 13) { document.location = 'Search.aspx?Query=' + encodeURI(document.getElementById('TxtSearchBox').value); return false; }"" /></nowiki>");
							break;
						case "FTSEARCHBOX":
							sb.Insert(match.Index, @"<nowiki><input type=""text"" id=""TxtSearchBox"" onkeydown=""javascript:var keycode; if(window.event) keycode = event.keyCode; else keycode = event.which; if(keycode == 10 || keycode == 13) { document.location = 'Search.aspx?FullText=1&amp;Query=' + encodeURI(document.getElementById('TxtSearchBox').value); return false; }"" /></nowiki>");
							break;
						case "PAGECOUNT":
							sb.Insert(match.Index, Pages.Instance.AllPages.Count.ToString());
							break;
						case "CLOUD":
							sb.Insert(match.Index, BuildCloud());
							break;
						 */
					}
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = specialTag.Match(sb.ToString(), end);
			}

			match = list.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					int d = 0;
					try
					{
						sb.Insert(match.Index, GenerateList(match.Value.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries), 0, 0, ref d) + "\n");
					}
					catch
					{
						sb.Insert(match.Index, @"<b style=""color: #FF0000;"">FORMATTER ERROR (Malformed List)</b>");
					}
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = list.Match(sb.ToString(), end);
			}

			match = hr.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					sb.Insert(match.Index, @"<h1 style=""border-bottom: solid 1px #999999;""> </h1>" + "\n");
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = hr.Match(sb.ToString(), end);
			}

			// Replace \n with BR was here

			ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);

			/*
			// Transclusion (intra-Wiki)
			match = transclusion.Match(sb.ToString());
			while(match.Success) {
				if(!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end)) {
					sb.Remove(match.Index, match.Length);
					PageInfo info = Pages.Instance.FindPage(match.Value.Substring(3, match.Value.Length - 4));
					if(info != null && info != current) { // Avoid circular transclusion!
						dummy = new StringBuilder();
						dummy.Append(@"<div class=""transcludedpage"">");
						// The current PageInfo is null to disable section editing and similar features
						dummy.Append(FormattingPipeline.FormatWithPhase1And2(Content.GetPageContent(info, true).Content, null));
						dummy.Append("</div>");
						sb.Insert(match.Index, dummy.ToString());
					}
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = transclusion.Match(sb.ToString(), end);
			}*/

			var attachments = new List<string>();

			// Links and images
			match = link.Match(sb.ToString());
			while (match.Success)
			{
				/*if(IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end)) {
					match = link.Match(sb.ToString(), end);
					continue;
				}*/
				if (match.Value.Equals("[]") || match.Value.Equals("[[]]")) continue; // Prevents formatting empty links
				done = false;
				tmp = match.Value.Trim('[', ']').Trim();
				sb.Remove(match.Index, match.Length);
				a = "";
				n = "";
				if (tmp.IndexOf("|") != -1)
				{
					// There are some fields
					string[] fields = tmp.Split('|');
					if (fields.Length == 2)
					{
						// Link with title
						a = fields[0];
						n = fields[1];
					}
					else
					{
						/*
						StringBuilder img = new StringBuilder();
						// Image
						if(fields[0].ToLowerInvariant().Equals("imageleft") || fields[0].ToLowerInvariant().Equals("imageright") || fields[0].ToLowerInvariant().Equals("imageauto")) {
							string c = "";
							switch(fields[0].ToLowerInvariant()) {
								case "imageleft":
									c = "imageleft";
									break;
								case "imageright":
									c = "imageright";
									break;
								case "imageauto":
									c = "imageauto";
									break;
								default:
									c = "image";
									break;
							}
							title = fields[1];
							url = fields[2];
							if(fields.Length == 4) bigUrl = fields[3];
							else bigUrl = "";
							url = EscapeUrl(url);
							// bigUrl = EscapeUrl(bigUrl); The url is already escaped by BuildUrl
							if(c.Equals("imageauto")) {
								img.Append(@"<table class=""imageauto"" cellpadding=""0"" cellspacing=""0"" align=""center""><tr><td>");
							}
							else {
								img.Append(@"<div class=""");
								img.Append(c);
								img.Append(@""">");
							}
							if(bigUrl.Length > 0) {
								dummy = new StringBuilder();
								dummy.Append(@"<img class=""image"" src=""");
								dummy.Append(url);
								dummy.Append(@""" alt=""");
								if(title.Length > 0) dummy.Append(StripWikiMarkup(StripHtml(title)));
								else dummy.Append("Image");
								dummy.Append(@""" />");
								img.Append(BuildLink(bigUrl, dummy.ToString(), true, title, lp));
							}
							else {
								img.Append(@"<img class=""image"" src=""");
								img.Append(url);
								img.Append(@""" alt=""");
								if(title.Length > 0) img.Append(StripWikiMarkup(StripHtml(title)));
								else img.Append("Image");
								img.Append(@""" />");
							}
							if(title.Length > 0) {
								img.Append(@"<p class=""imagedescription"">");
								img.Append(title);
								img.Append("</p>");
							}
							if(c.Equals("imageauto")) {
								img.Append("</td></tr></table>");
							}
							else {
								img.Append("</div>");
							}
							sb.Insert(match.Index, img);
						}
						else if(fields[0].StartsWith("image", StringComparison.InvariantCultureIgnoreCase)) {
							title = fields[1];
							url = fields[2];
							if(fields.Length == 4) bigUrl = fields[3];
							else bigUrl = "";
							url = EscapeUrl(url);
							// bigUrl = EscapeUrl(bigUrl); The url is already escaped by BuildUrl
							if(bigUrl.Length > 0) {
								dummy = new StringBuilder();
								dummy.Append(@"<img src=""");
								dummy.Append(url);
								dummy.Append(@""" alt=""");
								if(title.Length > 0) dummy.Append(StripWikiMarkup(StripHtml(title)));
								else dummy.Append("Image");
								dummy.Append(@""" />");
								img.Append(BuildLink(bigUrl, dummy.ToString(), true, title, lp));
							}
							else {
								img.Append(@"<img src=""");
								img.Append(url);
								img.Append(@""" alt=""");
								img.Append("Image");
								img.Append(@""" />");
							}
							sb.Insert(match.Index, img.ToString());
						}
						else {
							sb.Insert(match.Index, @"<b style=""color: #FF0000;"">FORMATTER ERROR (Malformed Image Tag)</b>");
						}*/
						done = true;
					}
				}
				else if (tmp.ToLowerInvariant().StartsWith("attachment:"))
				{
					// This is an attachment
					done = true;
					string f = tmp.Substring("attachment:".Length);
					if (f.StartsWith("{up}")) f = f.Substring(4);
					if (f.ToLowerInvariant().StartsWith(upReplacement.ToLowerInvariant())) f = f.Substring(upReplacement.Length);
					attachments.Add(UrlDecode(f));
					// Remove all trailing \n, so that attachments have no effect on the output in any case
					while (sb[match.Index] == '\n' && match.Index < sb.Length - 1)
					{
						sb.Remove(match.Index, 1);
					}
				}
				else
				{
					a = tmp;
					n = "";
				}
				if (!done)
				{

					var linkText = BuildLink(a, n, false, "");

					if (a.IndexOfAny("<>".ToCharArray()) != -1)
					{
						end = match.Index + linkText.Length;
					}

					sb.Insert(match.Index, linkText);
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = link.Match(sb.ToString(), Math.Min(end, sb.Length - 1));
			}

			match = boldItalic.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					dummy = new StringBuilder("<b><i>");
					dummy.Append(match.Value.Substring(5, match.Value.Length - 10));
					dummy.Append("</i></b>");
					sb.Insert(match.Index, dummy.ToString());
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = boldItalic.Match(sb.ToString(), end);
			}

			match = bold.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					dummy = new StringBuilder("<b>");
					dummy.Append(match.Value.Substring(3, match.Value.Length - 6));
					dummy.Append("</b>");
					sb.Insert(match.Index, dummy.ToString());
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = bold.Match(sb.ToString(), end);
			}

			match = italic.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					dummy = new StringBuilder("<i>");
					dummy.Append(match.Value.Substring(2, match.Value.Length - 4));
					dummy.Append("</i>");
					sb.Insert(match.Index, dummy.ToString());
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = italic.Match(sb.ToString(), end);
			}

			match = underlined.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					dummy = new StringBuilder("<u>");
					dummy.Append(match.Value.Substring(2, match.Value.Length - 4));
					dummy.Append("</u>");
					sb.Insert(match.Index, dummy.ToString());
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = underlined.Match(sb.ToString(), end);
			}

			match = striked.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					dummy = new StringBuilder("<strike>");
					dummy.Append(match.Value.Substring(2, match.Value.Length - 4));
					dummy.Append("</strike>");
					sb.Insert(match.Index, dummy.ToString());
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = striked.Match(sb.ToString(), end);
			}

			match = pre.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					dummy = new StringBuilder("<pre>");
					// IE needs \r\n for line breaks
					dummy.Append(match.Value.Substring(4, match.Value.Length - 8).Replace("\n", "\r\n"));
					dummy.Append("</pre>");
					sb.Insert(match.Index, dummy.ToString());
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = pre.Match(sb.ToString(), end);
			}

			match = code.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					// HACK: Removed this portion for Wiki pages to look better, trading off some info
					/*
					dummy = new StringBuilder("<code>");
					dummy.Append(match.Value.Substring(2, match.Value.Length - 4));
					dummy.Append("</code>");
					sb.Insert(match.Index, dummy.ToString());
					*/
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = code.Match(sb.ToString(), end);
			}

			string h;

			// Hx: detection pass (used for the TOC generation and section editing)
			hPos = DetectHeaders(sb.ToString());

			// Hx: formatting pass

			int count = 0;

			match = h4.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					h = match.Value.Substring(5, match.Value.Length - 10);
					dummy = new StringBuilder();
					dummy.Append(@"<a id=""");
					dummy.Append(BuildHAnchor(h, count.ToString()));
					dummy.Append(@"""></a><h4>");
					dummy.Append(h);
					dummy.Append("</h4>");
					sb.Insert(match.Index, dummy.ToString());
					count++;
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = h4.Match(sb.ToString(), end);
			}

			match = h3.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					h = match.Value.Substring(4, match.Value.Length - 8);
					dummy = new StringBuilder();
					if (current != null) dummy.Append(BuildEditSectionLink(count, current.Name));
					dummy.Append(@"<a id=""");
					dummy.Append(BuildHAnchor(h, count.ToString()));
					dummy.Append(@"""></a><h3 class=""separator"">");
					dummy.Append(h);
					dummy.Append("</h3>");
					sb.Insert(match.Index, dummy.ToString());
					count++;
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = h3.Match(sb.ToString(), end);
			}

			match = h2.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					h = match.Value.Substring(3, match.Value.Length - 6);
					dummy = new StringBuilder();
					if (current != null) dummy.Append(BuildEditSectionLink(count, current.Name));
					dummy.Append(@"<a id=""");
					dummy.Append(BuildHAnchor(h, count.ToString()));
					dummy.Append(@"""></a><h2 class=""separator"">");
					dummy.Append(h);
					dummy.Append("</h2>");
					sb.Insert(match.Index, dummy.ToString());
					count++;
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = h2.Match(sb.ToString(), end);
			}

			match = h1.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					h = match.Value.Substring(2, match.Value.Length - 4);
					dummy = new StringBuilder();
					if (current != null) dummy.Append(BuildEditSectionLink(count, current.Name));
					dummy.Append(@"<a id=""");
					dummy.Append(BuildHAnchor(h, count.ToString()));
					dummy.Append(@"""></a><h1 class=""separator"">");
					dummy.Append(h);
					dummy.Append("</h1>");
					sb.Insert(match.Index, dummy.ToString());
					count++;
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = h1.Match(sb.ToString(), end);
			}

			match = box.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					dummy = new StringBuilder(@"<div class=""box"">");
					dummy.Append(match.Value.Substring(3, match.Value.Length - 6));
					dummy.Append("</div>");
					sb.Insert(match.Index, dummy.ToString());
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = box.Match(sb.ToString(), end);
			}

			// "Disable" NoWiki'ed {Username} tags
			match = username.Match(sb.ToString());
			while (match.Success)
			{
				if (IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					sb.Remove(match.Index, match.Length);
					sb.Insert(match.Index, match.Value.Replace("{", "&#0123;").Replace("}", "&#0125;"));
				}
				else end = match.Index + match.Length + 1;
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = username.Match(sb.ToString(), end);
			}

			if (current != null)
			{
				var tocString = BuildToc(hPos);
				match = toc.Match(sb.ToString());
				while (match.Success)
				{
					if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
					{
						sb.Remove(match.Index, match.Length);
						sb.Insert(match.Index, tocString);
					}
					ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
					match = toc.Match(sb.ToString(), end);
				}
			}

			/*
			match = snippet.Match(sb.ToString());
			while(match.Success) {
				if(!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end)) {
					sb.Remove(match.Index, match.Length);
					int secondPipe = match.Value.Substring(3).IndexOf("|");
					string name = "";
					if(secondPipe == -1) name = match.Value.Substring(3, match.Length - 4);
					else name = match.Value.Substring(3, secondPipe);
					Snippet s = Snippets.Instance.Find(name);
					if(s != null) {
						string[] parameters = match.Value.Substring(3 + secondPipe + 1, match.Length - secondPipe - 5).Split('|');
						string fs = Format(PrepareSnippet(parameters, s.Content), null);
						fs = fs.TrimEnd('\n').TrimStart('\n');
						sb.Insert(match.Index, fs);
					}
					else {
						sb.Insert(match.Index, @"<b style=""color: #FF0000;"">FORMATTER ERROR (Snippet Not Found)</b>");
					}
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = snippet.Match(sb.ToString(), end);
			}*/

			// Remove <nowiki> tags
			sb.Replace("<nowiki>", "");
			sb.Replace("</nowiki>", "");

			// No more needed (Striked Regex modified)
			// Unescape comments
			//sb.Replace("($_^)", "<!--");
			//sb.Replace("(^_$)", "-->");

			// Setup BRs
			sb.Replace("\n\n", "<br /><br />");
			sb.Replace("<br>", "<br />");

			// Hacks
			sb.Replace("</ul><br /><br />", "</ul><br />");
			sb.Replace("</ol><br /><br />", "</ol><br />");
			sb.Replace("</table><br /><br />", "</table><br />");
			sb.Replace("</pre><br /><br />", "</pre><br />");
			sb.Replace("</div><br /><br />", "</div><br />");

			// Append Attachments
			if (attachments.Count > 0)
			{
				sb.Append(@"<div id=""AttachmentsDiv"">");
				for (int i = 0; i < attachments.Count; i++)
				{
					sb.Append(@"<a href=""");
					sb.Append(upReplacement);
					sb.Append(UrlEncode(attachments[i]));
					sb.Append(@""" class=""attachment"">");
					sb.Append(attachments[i]);
					sb.Append("</a>");
					if (i != attachments.Count - 1) sb.Append(" - ");
				}
				sb.Append("</div>");
			}

			if (surroundWithBody)
			{
				sb.Insert(0, "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"/>" +
				             "<meta name=\"DocId\" content=\"" + (current != null ? current.TopicId.ToString() : string.Empty) +
				             "\" />" +
				             "<title>" + HttpUtility.HtmlEncode(documentName) + "</title>" +
				             "</head><body" + (is_rtl ? " dir=\"rtl\" align=\"right\">" : ">"));
				sb.Append("</body></html>");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Prepares the content of a snippet, properly managing parameters.
		/// </summary>
		/// <param name="parameters">The snippet parameters.</param>
		/// <param name="snippet">The snippet original text.</param>
		/// <returns>The prepared snippet text.</returns>
		private static string PrepareSnippet(string[] parameters, string snippet)
		{
			var sb = new StringBuilder(snippet);

			for (int i = 0; i < parameters.Length; i++)
			{
				sb.Replace(string.Format("?{0}?", i + 1), parameters[i]);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Escapes all the characters used by the WikiMarkup.
		/// </summary>
		/// <param name="content">The Content.</param>
		/// <returns>The escaped Content.</returns>
		private static string EscapeWikiMarkup(string content)
		{
			StringBuilder sb = new StringBuilder(content);
			sb.Replace("&", "&amp;"); // Before all other escapes!
			sb.Replace("#", "&#0035;");
			sb.Replace("*", "&#0042;");
			sb.Replace("<", "&lt;");
			sb.Replace(">", "&gt;");
			sb.Replace("[", "&#0091;");
			sb.Replace("]", "&#0093;");
			sb.Replace("{", "&#0123;");
			sb.Replace("}", "&#0125;");
			sb.Replace("'''", "&#0039;&#0039;&#0039;");
			sb.Replace("''", "&#0039;&#0039;");
			sb.Replace("=====", "&#0061;&#0061;&#0061;&#0061;&#0061;");
			sb.Replace("====", "&#0061;&#0061;&#0061;&#0061;");
			sb.Replace("===", "&#0061;&#0061;&#0061;");
			sb.Replace("==", "&#0061;&#0061;");
			sb.Replace("§§", "&#0167;&#0167;");
			sb.Replace("__", "&#0095;&#0095;");
			sb.Replace("--", "&#0045;&#0045;");
			sb.Replace("@@", "&#0064;&#0064;");
			return sb.ToString();
		}

		/// <summary>
		/// Removes all the characters used by the WikiMarkup.
		/// </summary>
		/// <param name="content">The Content.</param>
		/// <returns>The stripped Content.</returns>
		private static string StripWikiMarkup(string content)
		{
			StringBuilder sb = new StringBuilder(content);
			sb.Replace("*", "");
			sb.Replace("<", "");
			sb.Replace(">", "");
			sb.Replace("[", "");
			sb.Replace("]", "");
			sb.Replace("{", "");
			sb.Replace("}", "");
			sb.Replace("'''", "");
			sb.Replace("''", "");
			sb.Replace("=====", "");
			sb.Replace("====", "");
			sb.Replace("===", "");
			sb.Replace("==", "");
			sb.Replace("§§", "");
			sb.Replace("__", "");
			sb.Replace("--", "");
			sb.Replace("@@", "");
			return sb.ToString();
		}

		/// <summary>
		/// Removes all HTML markup from a string.
		/// </summary>
		/// <param name="content">The string.</param>
		/// <returns>The result.</returns>
		private static string StripHtml(string content)
		{
			var sb = new StringBuilder(Regex.Replace(content, "<[^>]*>", " "));
			sb.Replace("&nbsp;", "");
			sb.Replace("  ", " ");
			return sb.ToString();
		}

		/// <summary>
		/// Builds a Link.
		/// </summary>
		/// <param name="a">The (raw) HREF.</param>
		/// <param name="n">The name/title.</param>
		/// <param name="isImage">True if the link contains an Image as "visible content".</param>
		/// <param name="imageTitle">The title of the image.</param>
		/// <param name="lp">The Linked Pages list.</param>
		/// <returns>The formatted Link.</returns>
		private static string BuildLink(string a, string n, bool isImage, string imageTitle/*, List<PageInfo> lp*/)
		{

			var blank = true; // default: false
			if (a.StartsWith("^"))
			{
				blank = true;
				a = a.Substring(1);
			}
			a = EscapeUrl(a);
			var nstripped = StripWikiMarkup(StripHtml(n));
			var imageTitleStripped = StripWikiMarkup(StripHtml(imageTitle));

			var sb = new StringBuilder();
			if (a.ToLowerInvariant().Equals("anchor") && n.StartsWith("#"))
			{
				sb.Append(@"<a id=""");
				sb.Append(n.Substring(1));
				sb.Append(@"""></a>");
			}
			else if (a.StartsWith("#"))
			{
				sb.Append(@"<a");
				if (!isImage) sb.Append(@" class=""internallink""");
				if (blank) sb.Append(@" target=""_blank""");
				sb.Append(@" href=""");
				sb.Append(a);
				sb.Append(@""" title=""");
				if (!isImage && n.Length > 0) sb.Append(nstripped);
				else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
				else sb.Append(a.Substring(1));
				sb.Append(@""">");
				if (n.Length > 0) sb.Append(n);
				else sb.Append(a.Substring(1));
				sb.Append("</a>");
			}
			else if (a.StartsWith("http://") || a.StartsWith("https://") || a.StartsWith("ftp://") || a.StartsWith("file://"))
			{
				// The link is complete
				// correctly handle external link descriptions (i.e. [http://www.moose.com Best Moose Site])
				int pos = a.IndexOf(" ");
				if (pos != -1)
				{
					// Link with title
					n = a.Substring(pos + 1);
					a = a.Substring(0, pos + 1);
				}
				sb.Append(@"<a");
				if (!isImage) sb.Append(@" class=""externallink""");
				sb.Append(@" href=""");
				sb.Append(a);
				sb.Append(@""" title=""");
				if (!isImage && n.Length > 0) sb.Append(nstripped);
				else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
				else sb.Append(a);
				sb.Append(@""" target=""_blank"">");
				if (n.Length > 0) sb.Append(n);
				else sb.Append(a);
				sb.Append("</a>");
			}
			else if (a.StartsWith(@"\\") || a.StartsWith("//"))
			{
				// The link is a UNC path
				sb.Append(@"<a");
				if (!isImage) sb.Append(@" class=""externallink""");
				sb.Append(@" href=""file://///");
				sb.Append(a.Substring(2));
				sb.Append(@""" title=""");
				if (!isImage && n.Length > 0) sb.Append(nstripped);
				else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
				else sb.Append(a);
				sb.Append(@""" target=""_blank"">");
				if (n.Length > 0) sb.Append(n);
				else sb.Append(a);
				sb.Append("</a>");
			}
			else if (a.IndexOf("@") != -1 && a.IndexOf(".") != -1)
			{
				// Email
				sb.Append(@"<a");
				if (!isImage) sb.Append(@" class=""emaillink""");
				if (blank) sb.Append(@" target=""_blank""");
				sb.Append(@" href=""mailto:");
				sb.Append(a.Replace("&amp;", "%26")); // Hack to let ampersands work in email addresses
				sb.Append(@""" title=""");
				if (!isImage && n.Length > 0) sb.Append(nstripped);
				else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
				else sb.Append(a);
				sb.Append(@""">");
				if (n.Length > 0) sb.Append(n);
				else sb.Append(a);
				sb.Append("</a>");
			}/*
			else if(((a.IndexOf(".") != -1 && !a.ToLowerInvariant().EndsWith(".aspx")) || a.EndsWith("/"))) {
				// Link to an internal file or subdirectory
				sb.Append(@"<a");
				if(!isImage) sb.Append(@" class=""internallink""");
				if(blank) sb.Append(@" target=""_blank""");
				sb.Append(@" href=""");
				sb.Append(a);
				sb.Append(@""" title=""");
				if(!isImage && n.Length > 0) sb.Append(nstripped);
				else if(isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
				else sb.Append(a);
				sb.Append(@""">");
				if(n.Length > 0) sb.Append(n);
				else sb.Append(a);
				sb.Append("</a>");
			}*/
			else
			{/*
				if(a.IndexOf(".aspx") != -1) {
					// The link points to a "system" page
					sb.Append(@"<a");
					if(!isImage) sb.Append(@" class=""systemlink""");
					if(blank) sb.Append(@" target=""_blank""");
					sb.Append(@" href=""");
					sb.Append(a);
					sb.Append(@""" title=""");
					if(!isImage && n.Length > 0) sb.Append(nstripped);
					else if(isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
					else sb.Append(a);
					sb.Append(@""">");
					if(n.Length > 0) sb.Append(n);
					else sb.Append(a);
					sb.Append("</a>");
				}
				else {
					if(a.StartsWith("c:") || a.StartsWith("C:")) {
						// Category link
						sb.Append(@"<a href=""AllPages.aspx?Cat=");
						sb.Append(Tools.UrlEncode(a.Substring(2)));
						sb.Append(@""" title=""");
						if(!isImage && n.Length > 0) sb.Append(nstripped);
						else if(isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
						else sb.Append(a.Substring(2));
						sb.Append(@""">");
						if(n.Length > 0) sb.Append(n);
						else sb.Append(a.Substring(2));
						sb.Append("</a>");
					}
					else if(a.Contains(":") || a.ToLowerInvariant().Contains("%3a") || a.Contains("&") || a.Contains("%26")) {
						sb.Append(@"<b style=""color: #FF0000;"">FORMATTER ERROR ("":"" and ""&"" not supported in Page Names)</b>");
					}
					else {
						// The link points to a wiki page
						string tempLink = a;
						if(a.IndexOf("#") != -1) {
							tempLink = a.Substring(0, a.IndexOf("#"));
							a = Tools.UrlEncode(a.Substring(0, a.IndexOf("#"))) + Settings.PageExtension + a.Substring(a.IndexOf("#"));
						}
						else {
							a += Settings.PageExtension;
							a = Tools.UrlEncode(a);
						}

						PageInfo info = Pages.Instance.FindPage(tempLink);
						if(info == null) {
							sb.Append(@"<a");
							if(!isImage) sb.Append(@" class=""unknownlink""");
							if(blank) sb.Append(@" target=""_blank""");
							sb.Append(@" href=""");
							sb.Append(a);
							sb.Append(@""" title=""");
							if(!isImage && n.Length > 0) sb.Append(nstripped);
							else if(isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
							else sb.Append(tempLink);
							sb.Append(@""">");
							if(n.Length > 0) sb.Append(n);
							else sb.Append(tempLink);
							sb.Append("</a>");
						}
						else {
							if(!lp.Contains(info)) {
								lp.Add(info);
							}*/

				// skip language referrals
				if (lang_link.IsMatch(a))
					return string.Empty;

				sb.Append(@"<a");
				if (!isImage) sb.Append(@" class=""pagelink""");
				if (blank) sb.Append(@" target=""_blank""");
				sb.Append(@" href=""");
				sb.Append("http://he.wikipedia.org/wiki/"); // TODO: Parameterize
				a = a.Replace("\"", "&quot;");
				sb.Append(a);
				sb.Append(@""" title=""");
				if (!isImage && n.Length > 0) sb.Append(nstripped);
				else if (isImage && imageTitle.Length > 0) sb.Append(imageTitleStripped);
				else sb.Append(/*Content.GetPageContent(info, false).Title*//*tempLink*/a);
				sb.Append(@""">");
				sb.Append(n.Length > 0 ? n : /*tempLink*/a);
				sb.Append("</a>");
				/*}
			}
		}*/
			}
			return sb.ToString();
		}

		/// <summary>
		/// Detects all the Headers in a block of text (H1, H2, H3, H4).
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>The List of Header objects, in the same order as they are in the text.</returns>
		public static List<HPosition> DetectHeaders(string text)
		{
			Match match;
			string h;
			int end = 0;
			List<int> noWikiBegin = new List<int>(), noWikiEnd = new List<int>();
			List<HPosition> hPos = new List<HPosition>();
			StringBuilder sb = new StringBuilder(text);

			ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);

			int count = 0;

			match = h4.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					h = match.Value.Substring(5, match.Value.Length - 10);
					hPos.Add(new HPosition(match.Index, h, 4, count));
					end = match.Index + match.Length;
					count++;
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = h4.Match(sb.ToString(), end);
			}

			match = h3.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					h = match.Value.Substring(4, match.Value.Length - 8);
					bool found = false;
					for (int i = 0; i < hPos.Count; i++)
					{
						if (match.Index == hPos[i].Index)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						hPos.Add(new HPosition(match.Index, h, 3, count));
						count++;
					}
					end = match.Index + match.Length;
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = h3.Match(sb.ToString(), end);
			}

			match = h2.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					h = match.Value.Substring(3, match.Value.Length - 6);
					bool found = false;
					for (int i = 0; i < hPos.Count; i++)
					{
						if (match.Index == hPos[i].Index)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						hPos.Add(new HPosition(match.Index, h, 2, count));
						count++;
					}
					end = match.Index + match.Length;
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = h2.Match(sb.ToString(), end);
			}

			match = h1.Match(sb.ToString());
			while (match.Success)
			{
				if (!IsNoWikied(match.Index, noWikiBegin, noWikiEnd, out end))
				{
					h = match.Value.Substring(2, match.Value.Length - 4);
					bool found = false;
					for (int i = 0; i < hPos.Count; i++)
					{
						// A special treatment is needed in this case
						// because =====xxx===== matches also 2 H1 headers (=='='==)
						if (match.Index >= hPos[i].Index && match.Index <= hPos[i].Index + hPos[i].Text.Length + 5)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						hPos.Add(new HPosition(match.Index, h, 1, count));
						count++;
					}
					end = match.Index + match.Length;
				}
				ComputeNoWiki(sb.ToString(), ref noWikiBegin, ref noWikiEnd);
				match = h1.Match(sb.ToString(), end);
			}

			return hPos;
		}

		private static string BuildEditSectionLink(int id, string page)
		{
			return String.Empty;
		}

		private static string GenerateList(string[] lines, int line, int level, ref int currLine)
		{
			StringBuilder sb = new StringBuilder();
			if (lines[currLine][level] == '*') sb.Append("<ul>");
			else if (lines[currLine][level] == '#') sb.Append("<ol>");
			while (currLine <= lines.Length - 1 && CountBullets(lines[currLine]) >= level + 1)
			{
				if (CountBullets(lines[currLine]) == level + 1)
				{
					sb.Append("<li>");
					sb.Append(lines[currLine].Substring(CountBullets(lines[currLine])).Trim());
					sb.Append("</li>");
					currLine++;
				}
				else
				{
					if (sb.Length >= 5)
						sb.Remove(sb.Length - 5, 5);
					sb.Append(GenerateList(lines, currLine, level + 1, ref currLine));
					sb.Append("</li>");
				}
			}
			if (lines[line][level] == '*') sb.Append("</ul>");
			else if (lines[line][level] == '#') sb.Append("</ol>");
			return sb.ToString();
		}

		private static int CountBullets(string line)
		{
			int res = 0, count = 0;
			while (line[count] == '*' || line[count] == '#')
			{
				res++;
				count++;
			}
			return res;
		}

		private static string ExtractBullets(string value)
		{
			string res = "";
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] == '*' || value[i] == '#') res += value[i];
				else break;
			}
			return res;
		}

		private static string BuildToc(List<HPosition> hPos)
		{
			StringBuilder sb = new StringBuilder();

			hPos.Sort(new HPositionComparer());

			// Table only used to workaround IE idiosyncrasies - use TocCointainer for styling
			sb.Append(@"<table id=""TocContainerTable""><tr><td>");
			sb.Append(@"<div id=""TocContainer"">");
			sb.Append(@"<p class=""small"">");
			sb.Append(tocTitlePlaceHolder);
			sb.Append("</p>");

			sb.Append(@"<div id=""Toc"">");
			sb.Append("<p><br />");
			for (int i = 0; i < hPos.Count; i++)
			{
				//Debug.WriteLine(i.ToString() + " " + hPos[i].Index.ToString() + ": " + hPos[i].Level);
				switch (hPos[i].Level)
				{
					case 1:
						break;
					case 2:
						sb.Append("&nbsp;&nbsp;&nbsp;");
						break;
					case 3:
						sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
						break;
					case 4:
						sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
						break;
				}
				if (hPos[i].Level == 1) sb.Append("<b>");
				if (hPos[i].Level == 4) sb.Append("<small>");
				sb.Append(@"<a href=""#");
				sb.Append(BuildHAnchor(hPos[i].Text, hPos[i].ID.ToString()));
				sb.Append(@""">");
				sb.Append(StripWikiMarkup(StripHtml(hPos[i].Text)));
				sb.Append("</a>");
				if (hPos[i].Level == 4) sb.Append("</small>");
				if (hPos[i].Level == 1) sb.Append("</b>");
				sb.Append("<br />");
			}
			sb.Append("</p>");
			sb.Append("</div>");

			sb.Append("</div>");
			sb.Append("</td></tr></table>");

			return sb.ToString();
		}

		/// <summary>
		/// Builds a valid anchor name from a string.
		/// </summary>
		/// <param name="h">The string, usually a header (Hx).</param>
		/// <returns>The anchor ID.</returns>
		public static string BuildHAnchor(string h)
		{
			StringBuilder sb = new StringBuilder(StripWikiMarkup(h));
			sb.Replace(" ", "_");
			sb.Replace(".", "");
			sb.Replace(",", "");
			sb.Replace("\"", "");
			sb.Replace("/", "");
			sb.Replace("\\", "");
			sb.Replace("'", "");
			sb.Replace("(", "");
			sb.Replace(")", "");
			sb.Replace("[", "");
			sb.Replace("]", "");
			sb.Replace("{", "");
			sb.Replace("}", "");
			sb.Replace("<", "");
			sb.Replace(">", "");
			sb.Replace("#", "");
			sb.Replace("\n", "");
			sb.Replace("?", "");
			sb.Replace("&", "");
			sb.Replace("0", "A");
			sb.Replace("1", "B");
			sb.Replace("2", "C");
			sb.Replace("3", "D");
			sb.Replace("4", "E");
			sb.Replace("5", "F");
			sb.Replace("6", "G");
			sb.Replace("7", "H");
			sb.Replace("8", "I");
			sb.Replace("9", "J");
			return sb.ToString();
		}

		/// <summary>
		/// Builds a valid and unique anchor name from a string.
		/// </summary>
		/// <param name="h">The string, usually a header (Hx).</param>
		/// <param name="uid">The unique ID.</param>
		/// <returns>The anchor ID.</returns>
		public static string BuildHAnchor(string h, string uid)
		{
			return BuildHAnchor(h) + "_" + uid;
		}

		/// <summary>
		/// Escapes ampersands in a URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The escaped URL.</returns>
		private static string EscapeUrl(string url)
		{
			return url.Replace("&", "&amp;");
		}

		/// <summary>
		/// Builds a HTML table from WikiMarkup.
		/// </summary>
		/// <param name="table">The WikiMarkup.</param>
		/// <returns>The HTML.</returns>
		private static string BuildTable(string table)
		{
			// Proceed line-by-line, ignoring the first and last one
			string[] lines = table.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			if (lines.Length < 3)
			{
				return "<b>FORMATTER ERROR (Malformed Table)</b>";
			}
			var sb = new StringBuilder();
			sb.Append("<table");
			if (lines[0].Length > 2)
			{
				sb.Append(" ");
				sb.Append(lines[0].Substring(3));
			}
			sb.Append(">");
			int count = 1;
			if (lines[1].Length >= 3 && lines[1].Trim().StartsWith("|+"))
			{
				// Table caption
				sb.Append("<caption>");
				sb.Append(lines[1].Substring(2));
				sb.Append("</caption>");
				count++;
			}
			if (!lines[count].StartsWith("|-")) sb.Append("<tr>");
			string item;
			for (int i = count; i < lines.Length - 1; i++)
			{
				if (lines[i].Trim().StartsWith("|-"))
				{
					// New line
					if (i != count) sb.Append("</tr>");
					sb.Append("<tr");
					if (lines[i].Length > 2)
					{
						sb.Append(" ");
						sb.Append(lines[i].Substring(2));
					}
					sb.Append(">");
				}
				else if (lines[i].Trim().StartsWith("|"))
				{
					// Cell
					if (lines[i].Length < 2) continue;
					item = lines[i].Substring(1);
					if (item.IndexOf(" || ") != -1)
					{
						sb.Append("<td>");
						sb.Append(item.Replace(" || ", "</td><td>"));
						sb.Append("</td>");
					}
					else if (item.IndexOf("|") != -1)
					{
						sb.Append("<td ");
						sb.Append(item.Substring(0, item.IndexOf("|")));
						sb.Append(">");
						sb.Append(item.Substring(item.IndexOf("|") + 1));
						sb.Append("</td>");
					}
					else
					{
						sb.Append("<td>");
						sb.Append(item);
						sb.Append("</td>");
					}
				}
				else if (lines[i].Trim().StartsWith("!"))
				{
					// Header
					if (lines[i].Length < 2) continue;
					item = lines[i].Substring(1);
					if (item.IndexOf(" !! ") != -1)
					{
						sb.Append("<th>");
						sb.Append(item.Replace(" !! ", "</th><th>"));
						sb.Append("</th>");
					}
					else if (item.IndexOf(" ! ") != -1)
					{
						sb.Append("<th ");
						sb.Append(item.Substring(0, item.IndexOf(" ! ")));
						sb.Append(">");
						sb.Append(item.Substring(item.IndexOf(" ! ") + 3));
						sb.Append("</th>");
					}
					else
					{
						sb.Append("<th>");
						sb.Append(item);
						sb.Append("</th>");
					}
				}
			}
			if (sb.ToString().EndsWith("<tr>"))
			{
				sb.Remove(sb.Length - 4 - 1, 4);
				sb.Append("</table>");
			}
			else
			{
				sb.Append("</tr></table>");
			}
			sb.Replace("<tr></tr>", "");

			return sb.ToString();
		}

		private static string BuildIndent(string indent)
		{
			int colons = 0;
			indent = indent.Trim();
			while (colons < indent.Length && indent[colons] == ':') colons++;
			indent = indent.Substring(colons).Trim();
			return @"<div style=""margin: 0px; padding: 0px; padding-left: " + ((int)(colons * 15)).ToString() + @"px"">" + indent + "</div>";
		}

		/*
		private static string BuildCloud() {
			StringBuilder sb = new StringBuilder();
			// Total categorized Pages (uncategorized Pages don't count)
			int tot = Pages.Instance.AllPages.Count - Pages.Instance.GetUncategorizedPages().Length;
			for(int i = 0; i < Pages.Instance.AllCategories.Count; i++) {
				if(Pages.Instance.AllCategories[i].Pages.Length > 0) {
					sb.Append(@"<a href=""AllPages.aspx?Cat=");
					sb.Append(Tools.UrlEncode(Pages.Instance.AllCategories[i].Name));
					sb.Append(@""" style=""font-size: ");
					sb.Append(ComputeSize((float)Pages.Instance.AllCategories[i].Pages.Length / (float)tot * 100F).ToString());
					sb.Append(@"px;"">");
					sb.Append(Pages.Instance.AllCategories[i].Name);
					sb.Append("</a>");
				}
				if(i != Pages.Instance.AllCategories.Count - 1) sb.Append(" ");
			}
			return sb.ToString();
		} */

		private static int ComputeSize(float percentage)
		{
			// Interpolates min and max size on a line, so that if:
			// - percentage = 0   -> size = minSize
			// - percentage = 100 -> size = maxSize
			// - intermediate values are calculated
			float minSize = 8, maxSize = 26;
			//return (int)((maxSize - minSize) / 100F * (float)percentage + minSize); // Linear interpolation
			return (int)(maxSize - (maxSize - minSize) * Math.Exp(-percentage / 25)); // Exponential interpolation
		}

		private static void ComputeNoWiki(string text, ref List<int> noWikiBegin, ref List<int> noWikiEnd)
		{
			noWikiBegin.Clear();
			noWikiEnd.Clear();

			var match = noWiki.Match(text);
			while (match.Success)
			{
				noWikiBegin.Add(match.Index);
				noWikiEnd.Add(match.Index + match.Length);
				match = noWiki.Match(text, match.Index + match.Length);
			}
		}

		private static bool IsNoWikied(int index, List<int> noWikiBegin, List<int> noWikiEnd, out int end)
		{
			for (int i = 0; i < noWikiBegin.Count; i++)
			{
				if (index > noWikiBegin[i] && index < noWikiEnd[i])
				{
					end = noWikiEnd[i];
					return true;
				}
			}
			end = 0;
			return false;
		}

		/// <summary>
		/// Executes URL-encoding, avoiding to use '+' for spaces.
		/// </summary>
		/// <remarks>This method uses internally Server.UrlEncode.</remarks>
		/// <param name="input">The input string.</param>
		/// <returns>The encoded string.</returns>
		public static string UrlEncode(string input)
		{
			return HttpUtility.UrlEncode(input).Replace("+", "%20");
		}

		/// <summary>
		/// Executes URL-encoding, avoiding to use '+' for spaces.
		/// </summary>
		/// <remarks>This method uses internally Server.UrlEncode.</remarks>
		/// <param name="input">The input string.</param>
		/// <returns>The encoded string.</returns>
		public static string UrlDecode(string input)
		{
			return HttpUtility.UrlDecode(input);
		}

		/*
				/// <summary>
				/// Performs the internal Phase 3 of the Formatting pipeline.
				/// </summary>
				/// <param name="raw">The raw data.</param>
				/// <param name="current">The current PageInfo, if any.</param>
				/// <returns>The formatted content.</returns>
				public static string FormatPhase3(string raw, PageInfo current) {
					StringBuilder sb = new StringBuilder();
					StringBuilder dummy;
					sb.Append(raw);

					dummy = new StringBuilder("<b>");
					dummy.Append(Exchanger.ResourceExchanger.GetResource("TableOfContents"));
					dummy.Append(@"</b><span id=""ExpandTocSpan""> [<a href=""#"" onclick=""javascript:if(document.getElementById('Toc').style['display']=='none') document.getElementById('Toc').style['display']=''; else document.getElementById('Toc').style['display']='none'; return false;"">");
					dummy.Append(Exchanger.ResourceExchanger.GetResource("HideShow"));
					dummy.Append("</a>]</span>");
					sb.Replace(tocTitlePlaceHolder, dummy.ToString());

					sb.Replace(editSectionPlaceHolder, Exchanger.ResourceExchanger.GetResource("Edit"));

					Match match;

					string shift = "";
					if(HttpContext.Current.Request.Cookies[Settings.CultureCookieName] != null) shift = HttpContext.Current.Request.Cookies[Settings.CultureCookieName]["T"];

					match = sign.Match(sb.ToString());
					while(match.Success) {
						sb.Remove(match.Index, match.Length);
						string txt = match.Value.Substring(3, match.Length - 6);
						int idx = txt.LastIndexOf(",");
						string[] fields = new string[] { txt.Substring(0, idx), txt.Substring(idx + 1) };
						dummy = new StringBuilder();
						dummy.Append(@"<span class=""signature""><a href=""Message.aspx?Username=");
						dummy.Append(Tools.UrlEncode(fields[0]));
						dummy.Append(@""">");
						dummy.Append(fields[0]);
						dummy.Append("</a>, ");
						dummy.Append(Tools.AlignWithPreferences(DateTime.Parse(fields[1]), shift).ToString(Settings.DateTimeFormat));
						dummy.Append("</span>");
						sb.Insert(match.Index, dummy.ToString());
						match = sign.Match(sb.ToString());
					}

					match = username.Match(sb.ToString());
					while(match.Success) {
						sb.Remove(match.Index, match.Length);
						if(SessionFacade.LoginKey != null) sb.Insert(match.Index, SessionFacade.Username);
						match = username.Match(sb.ToString());
					}

					return sb.ToString();
				}*/
	}

	/// <summary>
	/// Represents a Header.
	/// </summary>
	public class HPosition
	{

		private int index;
		private string text;
		private int level;
		private int id;

		/// <summary>
		/// Initializes a new instance of the <b>HPosition</b> class.
		/// </summary>
		/// <param name="index">The Index.</param>
		/// <param name="text">The Text.</param>
		/// <param name="level">The Header level.</param>
		/// <param name="id">The Unique ID of the Header (0-based counter).</param>
		public HPosition(int index, string text, int level, int id)
		{
			this.index = index;
			this.text = text;
			this.level = level;
			this.id = id;
		}

		/// <summary>
		/// Gets or sets the Index.
		/// </summary>
		public int Index
		{
			get { return index; }
			set { index = value; }
		}

		/// <summary>
		/// Gets or sets the Text.
		/// </summary>
		public string Text
		{
			get { return text; }
			set { text = value; }
		}

		/// <summary>
		/// Gets or sets the Level.
		/// </summary>
		public int Level
		{
			get { return level; }
			set { level = value; }
		}

		/// <summary>
		/// Gets or sets the ID (0-based counter).
		/// </summary>
		public int ID
		{
			get { return id; }
			set { id = value; }
		}

	}

	/// <summary>
	/// Compares HPosition objects.
	/// </summary>
	public class HPositionComparer : IComparer<HPosition>
	{
		/// <summary>
		/// Performs the comparison.
		/// </summary>
		/// <param name="x">The first object.</param>
		/// <param name="y">The second object.</param>
		/// <returns>The comparison result.</returns>
		public int Compare(HPosition x, HPosition y)
		{
			return x.Index.CompareTo(y.Index);
		}
	}
}
