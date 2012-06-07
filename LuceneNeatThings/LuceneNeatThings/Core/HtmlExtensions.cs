using System;
using System.Text;
using System.Web.Mvc;

namespace LuceneNeatThings.Core
{
	public static class HtmlExtensions
	{
		public static MvcHtmlString Pager(this HtmlHelper helper, int currentPage, byte currentPageSize, int totalRecords, string urlPrefix)
		{
			var sb1 = new StringBuilder();

			var seed = currentPage % currentPageSize == 0 ? currentPage : currentPage - (currentPage % currentPageSize);

			if (currentPage > 1)
				sb1.AppendLine(String.Format("<a href=\"{0}{1}\">Previous</a>", urlPrefix, currentPage - 1));

			if (currentPage - currentPageSize >= 0)
				sb1.AppendLine(String.Format("<a href=\"{0}{1}\">...</a>", urlPrefix, (currentPage - currentPageSize) + 1));

			for (var i = seed; i < Math.Round((totalRecords / 10) + 0.5) && i < seed + currentPageSize; i++)
			{
				if (i + 1 != currentPage)
					sb1.AppendLine(String.Format("<a href=\"{0}{1}\">{1}</a>", urlPrefix, i + 1));
				else
					sb1.AppendLine(String.Format("{0}", i + 1));
			}

			if (currentPage + currentPageSize <= (Math.Round((totalRecords / 10) + 0.5) - 1))
				sb1.AppendLine(String.Format("<a href=\"{0}{1}\">...</a>", urlPrefix, (currentPage + currentPageSize) + 1));

			if (currentPage < (Math.Round((totalRecords / 10) + 0.5) - 1))
				sb1.AppendLine(String.Format("<a href=\"{0}{1}\">Next</a>", urlPrefix, currentPage + 1));

			return MvcHtmlString.Create(sb1.ToString());
		}
	}
}