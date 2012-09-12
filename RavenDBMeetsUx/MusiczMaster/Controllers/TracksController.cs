using System.Linq;
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
            var query = RavenSession.Query<Track>("TracksIndex")
                .Customize(x => x.RandomOrdering());

            var tracks = query.Statistics(out stats)
                .Take(20)
                .ToList();

            return View(new TracksSearchViewModel
                            {
                                Tracks = tracks,
                                PageNumber = 1,
                                PageSize = 20,
                                ResultsCount = stats.TotalResults,
                                Message = "Showing all tracks, randomly sorted"
                            });
        }

        public ActionResult Year(int id)
        {
            RavenQueryStatistics stats;
            var query = RavenSession.Query<Track>("TracksIndex")
                .Where(x => x.Year == id);

            var tracks = query.Statistics(out stats)
                .Take(20)
                .ToList();

            return View("Index", new TracksSearchViewModel
            {
                Tracks = tracks,
                PageNumber = 1,
                PageSize = 20,
                ResultsCount = stats.TotalResults,
                Message = "Showing all tracks for year " + id
            });
        }

        public ActionResult Genre(string id)
        {
            RavenQueryStatistics stats;
            var query = RavenSession.Query<Track>("TracksIndex")
                .Where(x => x.Genre == id);

            var tracks = query.Statistics(out stats)
                .Take(20)
                .ToList();

            return View("Index", new TracksSearchViewModel
            {
                Tracks = tracks,
                PageNumber = 1,
                PageSize = 20,
                ResultsCount = stats.TotalResults,
                Message = "Showing all tracks for genre " + id,
            });
        }

        #region Facets

        public ActionResult Locate(string genre, string artist, int? yearFrom, int? yearTo, int? lengthFrom, int? lengthTo, string freeText)
        {
            RavenQueryStatistics stats;
            var query = RavenSession.Query<TrackHack>("TracksIndex");

            if (!string.IsNullOrWhiteSpace(genre))
                query = query.Where(x => x.Genre == genre);

            if (!string.IsNullOrWhiteSpace(artist))
                query = query.Where(x => x.Artist == artist);

            if (!string.IsNullOrWhiteSpace(freeText))
                query = query.Search(x => x.FreeText, freeText, 1, SearchOptions.And);

            if (yearFrom != null && yearTo != null)
                query = query.Where(x => x.Year >= yearFrom.Value && x.Year <= yearTo.Value);
            else if (yearFrom != null)
                query = query.Where(x => x.Year >= yearFrom.Value);
            else if (yearTo != null)
                query = query.Where(x => x.Year <= yearTo.Value);

            if (lengthFrom != null && lengthTo != null)
                query = query.Where(x => x.Length >= lengthFrom.Value && x.Length <= lengthTo.Value);
            else if (lengthFrom != null)
                query = query.Where(x => x.Length >= lengthFrom.Value);
            else if (lengthTo != null)
                query = query.Where(x => x.Length <= lengthTo.Value);

            var results = query.Statistics(out stats)
                .Take(20)
                .As<Track>() // also part of the TrackHack thingy
                .ToList();

            var facets = query.ToFacets("facets/Tracks");

            return View("Index", new TracksSearchViewModel
                                     {
                                         Tracks = results,
                                         PageNumber = 1,
                                         PageSize = 128,
                                         ResultsCount = stats.TotalResults,
                                         Facets = facets,
                                         Message = "Showing search results",
                                     });
        }

        // a dummy class, needed in order to use the Linq provider properly and not resort to LuceneQuery
        public class TrackHack : Track
        {
            public string FreeText { get; set; }
        }

        #endregion

        #region Search

        public ActionResult Search(string q)
        {
            RavenQueryStatistics stats;
            var query = RavenSession.Advanced.LuceneQuery<Track>("TracksIndex")
                .Search("FreeText", q);
                
            var tracks = query.Statistics(out stats)
                .Take(20)
                .ToList();

            return View("Index", new TracksSearchViewModel
            {
                Tracks = tracks,
                PageNumber = 1,
                PageSize = 20,
                ResultsCount = stats.TotalResults,
                Message = "Showing search results for ' " + q + " '",
            });
        }

        public ActionResult SearchEx(string q)
        {
            RavenQueryStatistics stats;
            var query = RavenSession.Query<TrackHack>("TracksIndex")
                .Search(x => x.FreeText, q);

            var tracks = query.Statistics(out stats)
                .Take(20)
                .As<Track>()
                .ToList();

            if (tracks.Count == 1)
            {
                // TODO: return the view Track page / add to playlist for tracks[0]
            }

            string msg = null;
            if (tracks.Count == 0)
            {
                var suggestions = query.Suggest();
                if (suggestions.Suggestions.Length == 1)
                    return SearchEx(suggestions.Suggestions[0]);

                msg = suggestions.Suggestions.Aggregate("No results found; did you mean: ", (current, suggestion) => current + (suggestion + ", ")) + " ?";
            }
            
            return View("Index", new TracksSearchViewModel
            {
                Tracks = tracks,
                PageNumber = 1,
                PageSize = 20,
                ResultsCount = stats.TotalResults,
                Message = msg ?? "Showing search results for ' " + q + " '",
            });
        }

        #endregion

        #region Interactive

        public ActionResult AutoComplete()
        {
            return View("Interactive");
        }

        public ActionResult AutoCompleteJson(string prefix)
        {
            if (prefix.Length < 1)
                return Json(new string[]{});

            var terms = RavenSession.Advanced.DocumentStore.DatabaseCommands.GetTerms("TracksIndex", "FreeText", prefix, 5);

            return Json(terms);
        }

        #endregion
    }
}
