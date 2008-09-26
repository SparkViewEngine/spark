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
using NorthwindDemo.Controllers;
using NUnit.Framework;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;

namespace NorthwindDemo.Tests
{
    [TestFixture]
    public class ProductsControllerTester
    {
        [Test]
        public void TestCompiledViewsWithJustEngine()
        {
            // Make the same settings the web app provides.
            // (Assemblies are forced to be added because many aren't loaded 
            // in the nunit appdomain.)
            SparkSettings settings = new SparkSettings()
                .SetDebug(false)
                .SetPageBaseType("Spark.Web.Mvc.SparkView");

            settings
                .AddNamespace("System")
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("System.Linq")
                .AddNamespace("System.Web.Mvc")
                .AddNamespace("Microsoft.Web.Mvc")
                .AddNamespace("NorthwindDemo.Models")
                .AddNamespace("NorthwindDemo.Views.Helpers");

            settings
                .AddAssembly("NorthwindDemo")
                .AddAssembly("Microsoft.Web.Mvc")
                .AddAssembly("Spark.Web.Mvc")
                .AddAssembly("System.Web.Mvc")
                .AddAssembly("System.Web.Routing, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            // create an engine
            var engine = new SparkViewEngine(settings)
                             {
                                 ViewFolder = new FileSystemViewFolder(@"..\..\..\NorthwindDemo\Views")
                             };

            // Generate and compile a bunch of views
            engine.CreateEntry(new SparkViewDescriptor()
                                   .SetTargetNamespace("NorthwindDemo.Controllers")
                                   .AddTemplate("Products/Edit.spark")
                                   .AddTemplate("Shared/Application.spark"));

            engine.CreateEntry(new SparkViewDescriptor()
                                   .SetTargetNamespace("NorthwindDemo.Controllers")
                                   .AddTemplate("Products/Categories.spark")
                                   .AddTemplate("Shared/Application.spark"));

            engine.CreateEntry(new SparkViewDescriptor()
                                   .SetTargetNamespace("NorthwindDemo.Controllers")
                                   .AddTemplate("Products/Detail.spark")
                                   .AddTemplate("Shared/Application.spark"));

            engine.CreateEntry(new SparkViewDescriptor()
                                   .SetTargetNamespace("NorthwindDemo.Controllers")
                                   .AddTemplate("Products/ListingByCategory.spark")
                                   .AddTemplate("Shared/Application.spark"));
        }

        [Test]
        public void TestViewsWithBatchPrecompile()
        {
            // Use the Global class to initialize settings
            var engines = new ViewEngineCollection();
            Global.RegisterViewEngine(engines);

            SparkViewFactory viewEngine = engines.OfType<SparkViewFactory>().First();
            viewEngine.ViewFolder = new FileSystemViewFolder(@"..\..\..\NorthwindDemo\Views");

            var batch = new SparkBatchDescriptor();
            batch.For<ProductsController>();
            viewEngine.Precompile(batch);
        }
    }
}