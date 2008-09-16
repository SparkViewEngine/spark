using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Spark.Web.Mvc;

namespace ClientRenderingViews
{
    public partial class Global
    {
        public static void RegisterViewEngine(ICollection<IViewEngine> engines)
        {
            engines.Add(new SparkViewFactory());
        }

        public static void RegisterRoutes(ICollection<RouteBase> routes)
        {
            routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
                           {
                               Defaults = new RouteValueDictionary(new { action = "Index", id = "" }),
                           });
        }
    }
}