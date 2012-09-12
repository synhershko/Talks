using System.Collections.Generic;
using XmcdParser;

namespace MusiczMaster.Core
{
    public class TrackSuggestionsViewModel
    {
        public Track Track { get; set; }
        public IEnumerable<Track> SuggestedTracks { get; set; }
    }
}