// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Spark.Compiler;

namespace Spark.Web.Mvc
{
    public class JavascriptViewResult : ActionResult
    {
        public string AreaName { get; set; }
        public string ViewName { get; set; }
        public string MasterName { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            var controllerName = context.RouteData.GetRequiredString("controller");

            if (string.IsNullOrEmpty(ViewName))
            {
                ViewName = context.RouteData.GetRequiredString("action");
            }
            
            var searchedLocations = new List<string>();
            var factories = ViewEngines.Engines.OfType<SparkViewFactory>();

            if (!factories.Any())
                throw new CompilerException("No SparkViewFactory instances are registered");

            foreach (var factory in factories)
            {
                var descriptor = factory.DescriptorBuilder.BuildDescriptor(
                    new BuildDescriptorParams("", controllerName, ViewName, MasterName, false, factory.DescriptorBuilder.GetExtraParameters(context)), searchedLocations);
                descriptor.Language = LanguageType.Javascript;
                var entry = factory.Engine.CreateEntry(descriptor);
                context.HttpContext.Response.ContentType = "text/javascript";
                context.HttpContext.Response.Write(entry.SourceCode);
                return;
            }

            throw new CompilerException("Unable to find templates at " +
                                            string.Join(", ", searchedLocations.ToArray()));
        }
    }
}
