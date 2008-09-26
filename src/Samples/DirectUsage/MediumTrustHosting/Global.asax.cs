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
using System.Reflection;
using System.Web;
using Spark;
using Spark.FileSystem;

namespace MediumTrustHosting
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var engine = new SparkViewEngine
                             {
                                 ViewFolder = new FileSystemViewFolder(Server.MapPath("~/Views"))
                             };

            Application["ViewEngine"] = engine;

            engine.LoadBatchCompilation(Assembly.Load("MediumTrustHosting.Views"));
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string path = Request.AppRelativeCurrentExecutionFilePath;
            if (string.Equals(path, "~/default.aspx", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(path, "~/"))
            {
                Context.RewritePath("~/Home.ashx");
            }
        }

        public static IList<SparkViewDescriptor> AllKnownDescriptors()
        {
            return new[]
                       {
                           Desc("home.spark", "master.spark"),
                           Desc("product.spark", "master.spark"),
                           Desc("productlist.spark", "master.spark"),
                       };
        }

        private static SparkViewDescriptor Desc(params string[] templates)
        {
            var desc = new SparkViewDescriptor();
            foreach (string template in templates)
                desc.AddTemplate(template);
            return desc;
        }
    }
}