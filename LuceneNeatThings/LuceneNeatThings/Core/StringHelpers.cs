using System.Web;

namespace LuceneNeatThings.Core
{
	public static class StringHelpers
	{
		public static string UrlEncode(this string val)
		{
			return HttpUtility.UrlEncode(val);
		}

		public static string UrlDecode(this string val)
		{
			return HttpUtility.UrlDecode(val);
		}

		public static string HtmlEncode(this string val)
		{
			return HttpUtility.HtmlEncode(val);
		}

		public static string HtmlDecode(this string val)
		{
			return HttpUtility.HtmlDecode(val);
		}
	}
}