using System;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Spatial;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Store;
using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Query;
using Spatial4n.Core.Shapes;

namespace SpatialDemo
{
	public class Program
	{
		static readonly SimpleSpatialFieldInfo fieldInfo =
			new SimpleSpatialFieldInfo("spatial_field_name");

		static readonly SpatialContext context =
			new SpatialContext(DistanceUnits.KILOMETERS);

		static SpatialStrategy<SimpleSpatialFieldInfo> strategy;

		public static void AddPoint(IndexWriter writer, string name, double lng, double lat)
		{
			Shape shape = context.MakePoint(lng, lat);
			var doc = new Document();
			foreach (var fld in strategy.CreateFields(fieldInfo, shape, true, false).Where(f => f != null))
			{
				doc.Add(fld);
			}
			doc.Add(new Field("Name", name, Field.Store.YES,
				Field.Index.NOT_ANALYZED_NO_NORMS));
			writer.AddDocument(doc);
		}

		static void Main(string[] args)
		{
			int maxLength = GeohashPrefixTree.GetMaxLevelsPossible();
			strategy = new RecursivePrefixTreeStrategy(
				new GeohashPrefixTree(context, maxLength));

			var dir = new RAMDirectory();
			var writer = new IndexWriter(dir, new SimpleAnalyzer(), true,
				IndexWriter.MaxFieldLength.UNLIMITED);

			AddPoint(writer, "London", -81.233040, 42.983390);
			AddPoint(writer, "East New York", -73.882360, 40.666770);
			AddPoint(writer, "Manhattan", -73.966250, 40.783430);
			AddPoint(writer, "New York City", -74.005970, 40.714270);
			AddPoint(writer, "Oslo", 10.746090, 59.912730);
			AddPoint(writer, "Bergen", 5.324150, 60.392990);
			AddPoint(writer, "Washington, D. C.", -77.036370, 38.895110);

			writer.Close();

			// Origin point - Oslo Spektrum
			const double lat = 59.9138688;
			const double lng = 10.752245399999993;
			const double radius = 600;
			var query = strategy.MakeQuery(new SpatialArgs(SpatialOperation.IsWithin,
				context.MakeCircle(lng, lat, radius)), fieldInfo);

			var searcher = new IndexSearcher(dir);
			var results = searcher.Search(query, null, 100);
			foreach (var topDoc in results.ScoreDocs)
			{
				var name = searcher.Doc(topDoc.doc).Get("Name");
				Console.WriteLine(name);
			}
			searcher.Close();
			dir.Close();
		}
	}
}
