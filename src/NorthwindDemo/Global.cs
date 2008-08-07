using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.SparkViewEngine;
using Spark;

namespace NorthwindDemo
{
    public partial class Global
    {
        public static void RegisterControllerFactory(ControllerBuilder builder)
        {
            var settings = new SparkSettings()
                .SetDebug(true)
                .AddNamespace("System")
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("System.Linq")
                .AddNamespace("System.Web.Mvc")
                .AddNamespace("NorthwindDemo.Models")
                .AddNamespace("NorthwindDemo.Views.Helpers");

            builder.SetControllerFactory(new SparkControllerFactory { Settings = settings });
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            // Note: Change the URL to "{controller}.mvc/{action}/{id}" to enable
            //       automatic support on IIS6 and IIS7 classic mode

            routes.MapRoute("mvcroute", "{controller}/{action}/{id}"
                , new { controller = "products", action = "Index", id = "" }
                , new { controller = @"[^\.]*" });
        }
    }
}
