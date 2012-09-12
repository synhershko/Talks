using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Raven.Json.Linq;
using XmcdParser;

namespace MusiczMaster.Core
{
    public static class YouTubeResolver
    {
        public static MvcHtmlString YouTubeLink(this Track t)
        {
            try
            {
                var queryUrl =
                    string.Format(
                        "http://gdata.youtube.com/feeds/api/videos?q={0}&v=2&alt=jsonc&quality=medium&max-results=1",
                        HttpContext.Current.Server.UrlEncode(t.Artist + " - " + t.Title));

                var w = new WebClient();
                var obj = RavenJObject.Parse(w.DownloadString(queryUrl));

                var firstResult = obj.Value<RavenJObject>("data").Value<RavenJArray>("items")[0] as RavenJObject;

                if (firstResult != null)
                {
                    return
                        MvcHtmlString.Create(
                            string.Format(
                                @"<a href=""http://www.youtube.com/watch?v={0}"" rel=""_blank""><img src=""{1}"" /></a>",
                                firstResult.Value<string>("id"),
                                firstResult.Value<RavenJObject>("thumbnail").Value<string>("sqDefault"))
                            );
                }
            }
            catch (Exception)
            {
            }
            return MvcHtmlString.Empty;
        }
    }
}