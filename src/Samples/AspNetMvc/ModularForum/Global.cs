using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;

namespace ModularForum
{
    public partial class Global
    {
        public static void RegisterViewEngine(ICollection<IViewEngine> engines)
        {
            var settings = new SparkSettings()
                .SetDebug(true)
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("Microsoft.Web.Mvc")
                .AddNamespace("ModularForum.Controllers")
                .AddNamespace("ModularForum.Models");

            var engine = new SparkViewFactory(settings)
                             {
                                 ViewFolder = new EmbeddedViewFolder(Assembly.Load("ModularForum"), "ModularForum.Views")
                             };
            engines.Add(engine);
        }

        public static void RegisterRoutes(ICollection<RouteBase> routes)
        {
            routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
                           {
                               Constraints = new RouteValueDictionary(new {controller = "Forum"}),
                               Defaults = new RouteValueDictionary(new { action = "Index", id = "" }),
                           });
        }
    }
}
