using System.Collections.Generic;
using XmcdParser;

namespace MusiczMaster.Core
{
    public class TracksSearchViewModel
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int ResultsCount { get; set; }

        public string Message { get; set; }

        public IEnumerable<Track> Tracks { get; set; }
    }
}