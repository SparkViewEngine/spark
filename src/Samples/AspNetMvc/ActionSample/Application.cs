using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spark.Web.Mvc;
using System.Web.Routing;

namespace ActionSample
{
    public class Application
    {
        public void RegisterRoutes(IList<RouteBase> routes)
        {
            routes.Add(new Route(
                "{controller}/{action}/{id}",
                new RouteValueDictionary(new { controller = "home", action = "index", id = "" }),
                new MvcRouteHandler()));
        }
        public void RegisterViewEngine(IList<IViewEngine> engines)
        {
            SparkEngineStarter.RegisterViewEngine(engines);
        }
    }
}
