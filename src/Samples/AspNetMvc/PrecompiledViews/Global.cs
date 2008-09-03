using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Spark.Web.Mvc;

namespace PrecompiledViews
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

        public static void LoadPrecompiledViews(ViewEngineCollection engines)
        {
            var factory = engines.OfType<SparkViewFactory>().First();
            factory.Engine.LoadBatchCompilation(Assembly.Load("Precompiled"));
        }
    }
}