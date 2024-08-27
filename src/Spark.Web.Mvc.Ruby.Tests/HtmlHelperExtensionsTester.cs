// Copyright 2008-2024 Louis DeJardin
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

using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Web.Mvc.Extensions;

namespace Spark.Web.Mvc.Ruby.Tests
{
    [TestFixture]
    public class HtmlHelperExtensionsTester
    {
        public class StubHttpContext : HttpContextBase
        {
            public override Cache Cache => null;
        }

        public class StubController : Controller
        {
        }

        [Test]
        public void BuildingScriptHeader()
        {
            var settings = new SparkSettings();

            var batchCompiler = new RoslynBatchCompiler(settings);

            var languageFactory = new RubyLanguageFactoryWithExtensions(batchCompiler, settings);

            var header = languageFactory.BuildScriptHeader(languageFactory.GetType().Assembly);

            Assert.That(header.Contains("ActionLink"));
            Assert.That(header.Contains("Password"));
            Assert.That(header.Contains("RenderPartial"));
        }

        private static ViewContext CompileView(string viewContents)
        {
            var services = new ServiceCollection()
                .AddSpark(new SparkSettings());
            
            var viewFolder = new InMemoryViewFolder { { $"stub{Path.DirectorySeparatorChar}index.spark", viewContents } };

            services.AddSingleton<IViewFolder>(viewFolder);

            var serviceProvider = services.BuildServiceProvider();

            var viewEngine = serviceProvider.GetService<SparkViewFactory>();

            var httpContext = new StubHttpContext();

            var routeData = new RouteData();
            routeData.Values.Add("controller", "stub");
            routeData.Values.Add("action", "index");

            var controller = new StubController();
            var controllerContext = new ControllerContext(httpContext, routeData, controller);

            var result = viewEngine.FindPartialView(controllerContext, "index", false);

            return new ViewContext(controllerContext, result.View, new ViewDataDictionary(), new TempDataDictionary(), new StringWriter());
        }

        [Test]
        public void RenderSimpleView()
        {
            var viewContents = "<p>Hello world</p>";

            var viewContext = CompileView(viewContents);

            var output = new StringWriter();
            viewContext.View.Render(viewContext, output);

            Assert.AreEqual("<p>Hello world</p>", output.ToString());
        }

        [Test, Ignore("MVC 3.0 bringing some complication in relation to IronRuby - need to investigate")]
        public void UseExtensionMethod()
        {
            var viewContents = "<p>${html.TextBox('hello','world')}</p>";

            var viewContext = CompileView(viewContents);

            var output = new StringWriter();
            viewContext.View.Render(viewContext, output);

            Assert.That(output.ToString().StartsWith("<p><input"));
            Assert.That(output.ToString().Contains("name=\"hello\""));
            Assert.That(output.ToString().Contains("type=\"text\""));
        }
    }
}
