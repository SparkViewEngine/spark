using System.Web.Mvc;
using System.Web.Routing;
using Spark.Web.Mvc;

namespace PartialFiles
{
    public partial class Global
    {
        public static void RegisterViewEngine(ViewEngineCollection engines)
        {
            engines.Add(new SparkViewFactory());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(new { action = "Index", id = "" }),
            });

            routes.Add(new Route("Default.aspx", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(new { controller = "Home", action = "Index", id = "" }),
            });
        }
    }
}
