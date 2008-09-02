using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.SparkViewEngine;
using NorthwindDemo.Controllers;
using Spark;
using System.Linq;

namespace NorthwindDemo
{
    public partial class Global
    {
        public static void RegisterViewEngine(ViewEngineCollection engines)
        {
            var settings = new SparkSettings()
                .SetDebug(true)
                .AddNamespace("System")
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("System.Linq")
                .AddNamespace("System.Web.Mvc")
                .AddNamespace("Microsoft.Web.Mvc")
                .AddNamespace("NorthwindDemo.Models")
                .AddNamespace("NorthwindDemo.Views.Helpers");

            var spark = new SparkViewFactory(settings);
            engines.Add(spark);
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            // Note: Change the URL to "{controller}.mvc/{action}/{id}" to enable
            //       automatic support on IIS6 and IIS7 classic mode

            routes.MapRoute("mvcroute", "{controller}/{action}/{id}"
                , new { controller = "products", action = "Index", id = "" }
                , new { controller = @"[^\.]*" });
        }

        public static void PrecompileViews(ViewEngineCollection engines)
        {
            try
            {
                var viewFactory = engines.OfType<SparkViewFactory>().First();

                var batch = new SparkBatchDescriptor();

                batch
                    .For<HomeController>()
                    .For<ProductsController>();

                viewFactory.Precompile(batch);
            }
            catch
            {
                // the sample has a DropDownList compile error at the moment
            }
        }
    }
}
