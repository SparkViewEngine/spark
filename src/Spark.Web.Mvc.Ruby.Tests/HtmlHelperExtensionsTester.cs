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
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Web.Mvc.Ruby.Tests
{
    [TestFixture]
    public class HtmlHelperExtensionsTester
    {
        public class StubHttpContext : HttpContextBase
        {
            public override Cache Cache { get { return null; } }
        }

        public class StubController : Controller
        {

        }

        [Test]
        public void BuildingScriptHeader()
        {
            var languageFactory = new RubyLanguageFactoryWithExtensions();
            var header = languageFactory.BuildScriptHeader(languageFactory.GetType().Assembly);
            Assert.That(header.Contains("ActionLink"));
            Assert.That(header.Contains("Password"));
            Assert.That(header.Contains("RenderPartial"));
        }


        private static ViewContext CompileView(string viewContents)
        {
            var settings = new SparkSettings();
            var container = SparkRubyEngineStarter.CreateContainer(settings);

            var viewFolder = new InMemoryViewFolder { { string.Format("stub{0}index.spark", Path.DirectorySeparatorChar), viewContents } };
            container.SetServiceBuilder<IViewFolder>(c => viewFolder);
            var viewEngine = container.GetService<IViewEngine>();

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
