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
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;

namespace ModularForum
{
    public partial class Global
    {
        public static void RegisterViewEngine(ICollection<IViewEngine> engines)
        {
            SparkSettings settings = new SparkSettings()
                .SetDebug(true)
                .SetAutomaticEncoding(true)
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("Microsoft.Web.Mvc")
                .AddNamespace("ModularForum.Controllers")
                .AddNamespace("ModularForum.Models");

            var engine = new SparkViewFactory(settings)
                             {
                                 ViewFolder =
                                     new EmbeddedViewFolder(Assembly.Load("ModularForum"), "ModularForum.Views")
                             };
            engines.Add(engine);
        }

        public static void RegisterRoutes(ICollection<RouteBase> routes)
        {
            routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
                           {
                               Constraints = new RouteValueDictionary(new {controller = "Forum"}),
                               Defaults = new RouteValueDictionary(new {action = "Index", id = ""}),
                           });
        }
    }
}