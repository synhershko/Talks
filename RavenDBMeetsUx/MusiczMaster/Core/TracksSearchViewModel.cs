using System.Collections.Generic;
using Raven.Abstractions.Data;
using XmcdParser;

namespace MusiczMaster.Core
{
    public class TracksSearchViewModel
    {
        public TracksSearchViewModel()
        {
            PageNumber = 1;
            PageSize = 20;
        }

        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int ResultsCount { get; set; }

        public string Message { get; set; }
        public FacetResults Facets { get; set; }

        public IEnumerable<Track> Tracks { get; set; }
    }
}