using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Spark.Web.Mvc;
using Spark.Web.Mvc.Descriptors;

namespace SkinsFolder
{
    public class Application
    {
        public void RegisterViewEngine(ICollection<IViewEngine> engines)
        {
            var services = SparkEngineStarter.CreateContainer();
            services.AddFilter(ThemeDescriptorFilter.For(GetTheme));
            SparkEngineStarter.RegisterViewEngine(engines, services);
        }

        private static string GetTheme(ControllerContext context)
        {
            var cookie = context.HttpContext.Request.Cookies["theme"];
            return cookie == null ? null : cookie.Value;
        }

        public void RegisterRoutes(ICollection<RouteBase> routes)
        {
            routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
                       {
                           Defaults = new RouteValueDictionary(new { action = "Index", id = "" }),
                       });
        }
    }
}
