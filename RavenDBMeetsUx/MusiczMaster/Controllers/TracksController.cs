using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MusiczMaster.Core;
using Raven.Client.Linq;
using XmcdParser;

namespace MusiczMaster.Controllers
{
    public class TracksController : RavenController
    {
        public ActionResult Index()
        {
            RavenQueryStatistics stats;
            var tracks = RavenSession.Query<Track>("TracksIndex")
                .Customize(x => x.RandomOrdering())
                .Statistics(out stats)
                .ToList();

            return View(new TracksSearchViewModel
                            {
                                Tracks = tracks,
                                PageNumber = 1,
                                PageSize = 128,
                                ResultsCount = stats.TotalResults,
                                Message = "Showing all tracks, randomly sorted"
                            });
        }

        public ActionResult Year(int id)
        {
            RavenQueryStatistics stats;
            var tracks = RavenSession.Query<Track>("TracksIndex")
                .Where(x => x.Year == id)
                .Statistics(out stats)
                .ToList();

            return View("Index", new TracksSearchViewModel
            {
                Tracks = tracks,
                PageNumber = 1,
                PageSize = 128,
                ResultsCount = stats.TotalResults,
                Message = "Showing all tracks for year " + id
            });
        }

        public ActionResult Genre(string id)
        {
            RavenQueryStatistics stats;
            var tracks = RavenSession.Query<Track>("TracksIndex")
                .Where(x => x.Genre == id)
                .Statistics(out stats)
                .ToList();

            return View("Index", new TracksSearchViewModel
            {
                Tracks = tracks,
                PageNumber = 1,
                PageSize = 128,
                ResultsCount = stats.TotalResults,
                Message = "Showing all tracks for genre " + id,
            });
        }
    }
}
