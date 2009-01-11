// Copyright 2008 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using NorthwindDemo.Controllers;
using Spark;
using Spark.Web.Mvc;

namespace NorthwindDemo
{
    public partial class Global
    {
        public static void RegisterViewEngine(ViewEngineCollection engines)
        {
            var settings = new SparkSettings();
            settings.SetAutomaticEncoding(true);

            settings
                .AddNamespace("System")
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("System.Linq")
                .AddNamespace("System.Web.Mvc")
                .AddNamespace("System.Web.Mvc.Html")
                .AddNamespace("Microsoft.Web.Mvc")
                .AddNamespace("NorthwindDemo.Models")
                .AddNamespace("NorthwindDemo.Views.Helpers");

            settings
                .AddAssembly("Microsoft.Web.Mvc")
                .AddAssembly("Spark.Web.Mvc")
                .AddAssembly("System.Web.Mvc, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")
                .AddAssembly("System.Web.Routing, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            engines.Add(new SparkViewFactory(settings));
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            // Note: Change the URL to "{controller}.mvc/{action}/{id}" to enable
            //       automatic support on IIS6 and IIS7 classic mode

            routes.MapRoute("mvcroute", "{controller}/{action}/{id}"
                            , new {controller = "products", action = "Index", id = ""}
                            , new {controller = @"[^\.]*"});
        }

        public static void PrecompileViews(ViewEngineCollection engines)
        {
            try
            {
                SparkViewFactory viewFactory = engines.OfType<SparkViewFactory>().First();

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