using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Spark.Web.Mvc;

namespace CachingViewHunks
{
    public class Application
    {
        public void RegisterViewEngine(ICollection<IViewEngine> engines)
        {
            SparkEngineStarter.RegisterViewEngine(engines);
        }

        public void RegisterRoutes(ICollection<RouteBase> routes)
        {
            routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(new { controller = "Home", action = "Index", id = "" }),
            });
        }

        public static int FetchEmployeeListCalls { get; set; }
        public static int FetchEmployeeDetailCalls { get; set; }
    }
}
