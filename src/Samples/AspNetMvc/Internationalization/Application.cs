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
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Spark;
using Spark.Web.Mvc;
using Spark.Web.Mvc.Descriptors;

namespace Internationalization
{
    public class Application
    {
        public void RegisterViewEngine(ICollection<IViewEngine> engines)
        {
            SparkEngineStarter.RegisterViewEngine(engines);
            LanguageKit.Install(engines, GetSessionCulture);
        }

        public static string GetSessionCulture(ControllerContext controllerContext)
        {
            return Convert.ToString(controllerContext.HttpContext.Session["culture"]);
        }

        public static void SetSessionCulture(ControllerContext controllerContext, string culture)
        {
            controllerContext.HttpContext.Session["culture"] = culture;
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