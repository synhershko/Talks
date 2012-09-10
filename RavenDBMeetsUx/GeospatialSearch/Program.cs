using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace GeospatialSearch
{
	public class Event
	{
		public string Title { get; set; }
		public string Location { get; set; } // ex. POINT (24.532341 54.352753)
	}

	//public class LegacySpatialIndex : AbstractIndexCreationTask<Event>
	//{
	//    public LegacySpatialIndex()
	//    {
	//        Map = docs => from doc in docs
	//                      select new
	//                      {
	//                          doc.Title,
	//                          _ = SpatialGenerate(doc.lat, doc.lng)
	//                      };
	//    }
	//}

	public class SpatialIndex : AbstractIndexCreationTask<Event>
	{
		public SpatialIndex()
		{
			Map = docs => from doc in docs
			              select new
			                     	{
										doc.Title,
										_ = SpatialGenerate("Location", doc.Location,
												SpatialSearchStrategy.GeohashPrefixTree, 12)
			                     	};
		}
	}

	public class Program
	{
		private static IDocumentStore store;

		static void Main(string[] args)
		{
			store = new DocumentStore() {ConnectionStringName="RavenDB"};


		}

		public static IEnumerable<Event> GetEvents()
		{
			Polygon a = new Polygon(new LinearRing(new[] { new Coordinate(23.352, 52.32), new Coordinate(63.352, 82.32) }));
			a.ToString();

			IEnumerable<Event> events;
			using (var session = store.OpenSession())
			{
				events = session.Query<Event>()
					.Customize(x => x.RelatesToShape("Location", "Circle(32.454898 53.234012 d=6.000000)", SpatialRelation.Within))
					.ToList();

			}
			return events;
		}

		public static IEnumerable<Event> GetEventsLegacy()
		{
			IEnumerable<Event> events;
			using (var session = store.OpenSession())
			{
				events = session.Query<Event>()
					.Customize(x => x.WithinRadiusOf(10, 32.456236, 54.234053))
					.ToList();
			}
			return events;
		}
	}
}
