using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Spark.Web.Mvc;

namespace AdditionalViewFolders
{
    public partial class Global
    {
        public static void RegisterViewEngine(ICollection<IViewEngine> engines)
        {
            var engine = new SparkViewFactory();
            engine.AddSharedPath("~/ExtraCommon");
            engine.AddLayoutsPath("~/Masters");
            engine.AddEmbeddedResources(Assembly.Load("AdditionalViewResources"), "AdditionalViewResources.MoreViews");
            engines.Add(engine);
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