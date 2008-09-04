using System.Web.Mvc;
using System.Web.Routing;
using NorthwindDemo.Controllers;
using Spark;
using System.Linq;
using Spark.FileSystem;
using Spark.Web.Mvc;

namespace NorthwindDemo
{
    public partial class Global
    {
        public static void RegisterViewEngine(ViewEngineCollection engines)
        {
            var settings = new SparkSettings();
            
            settings
                .AddNamespace("System")
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("System.Linq")
                .AddNamespace("System.Web.Mvc")
                .AddNamespace("Microsoft.Web.Mvc")
                .AddNamespace("NorthwindDemo.Models")
                .AddNamespace("NorthwindDemo.Views.Helpers");

            settings
                .AddAssembly("Microsoft.Web.Mvc")
                .AddAssembly("Spark.Web.Mvc")
                .AddAssembly("System.Web.Mvc")
                .AddAssembly("System.Web.Routing, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            engines.Add(new SparkViewFactory(settings));
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
