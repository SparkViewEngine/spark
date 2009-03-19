using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Spark.Web.Mvc.Python;

namespace IronPythonViews
{
    public partial class Global
    {
        public static void RegisterViewEngine(ICollection<IViewEngine> engines)
        {
            SparkPythonEngineStarter.RegisterViewEngine(engines);
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