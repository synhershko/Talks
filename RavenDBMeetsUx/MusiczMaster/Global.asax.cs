using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace MusiczMaster
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        public static IDocumentStore DocumentStore { get; private set; }


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            DocumentStore = new DocumentStore {ConnectionStringName = "RavenDB"}.Initialize();
            IndexCreation.CreateIndexes(typeof(MvcApplication).Assembly, DocumentStore);

            SetupFacets();
        }

        private static void SetupFacets()
        {
            using (var session = DocumentStore.OpenSession())
            {
                session.Store(new FacetSetup
                                  {
                                      Id = "facets/Tracks",
                                      Facets = new List<Facet>
                                                   {
                                                       new Facet {Name = "Artist", TermSortMode = FacetTermSortMode.HitsDesc},
                                                       new Facet {Name = "Genre", TermSortMode = FacetTermSortMode.HitsDesc},
                                                       new Facet {Name = "Year", TermSortMode = FacetTermSortMode.HitsDesc},
                                                       new Facet
                                                           {
                                                               Name = "Length_Range",
                                                               TermSortMode = FacetTermSortMode.ValueAsc,
                                                               Mode = FacetMode.Ranges,
                                                               Ranges =
                                                                   {
                                                                       "[NULL TO Ix180]",
                                                                       "[Ix181 TO Ix210]",
                                                                       "[Ix211 TO Ix300]",
                                                                       "[Ix301 TO NULL]"
                                                                   }
                                                           },
                                                   }
                                  });
                session.SaveChanges();
            }
        }
    }
}