using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using XmcdParser;

namespace Muzics.Core
{
    public class TracksIndex : AbstractIndexCreationTask<Track>
    {
        public TracksIndex()
        {
            Map = docs => from doc in docs
                          select new
                                     {
                                         doc.Title,
                                         doc.Artist,
                                         doc.Genre,
                                         doc.Year,
                                         doc.Length,
                                         doc.AlbumId,
                                         doc.TrackNo,
                                         FreeText = new object[] {doc.Title, doc.Artist, doc.Genre, doc.Year}
                                     };
            
            Sort(x => x.TrackNo, SortOptions.Byte);
            Sort(x => x.Year, SortOptions.Short);
            Sort(x => x.Length, SortOptions.Short);

            Index("FreeText", FieldIndexing.Analyzed);
        }
    }
}