using System.Web.Mvc;
using MusiczMaster.Core;
using Muzics.Core;
using Raven.Abstractions.Data;
using Raven.Client.Bundles.MoreLikeThis;
using XmcdParser;

namespace MusiczMaster.Controllers
{
    public class PlaylistsController : RavenController
    {
        public ActionResult AddToPlaylist(int? id, string track)
        {
            var obj = RavenSession.Load<Track>(track);
            if (obj == null)
                return HttpNotFound();

            var mlt = RavenSession.Advanced.MoreLikeThis<Track, TracksIndex>(new MoreLikeThisQueryParameters
                                                                                 {
                                                                                     DocumentId = track,
                                                                                     Fields = new[]{"FreeText"},
                                                                                 });

            return View(new TrackSuggestionsViewModel
                            {
                                Track = obj,
                                SuggestedTracks = mlt,
                            });
        }
    }
}
