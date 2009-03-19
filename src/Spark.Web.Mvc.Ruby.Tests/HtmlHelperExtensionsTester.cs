using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
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
            CompiledViewHolder.Current = new CompiledViewHolder();
            
            var settings = new SparkSettings();
            var container = SparkRubyEngineStarter.CreateContainer(settings);

            var viewFolder = new InMemoryViewFolder { { "stub\\index.spark", viewContents } };
            container.SetServiceBuilder<IViewFolder>(c => viewFolder);
            var viewEngine = container.GetService<IViewEngine>();

            var httpContext = new StubHttpContext();


            var routeData = new RouteData();
            routeData.Values.Add("controller", "stub");
            routeData.Values.Add("action", "index");

            var controller = new StubController();
            var controllerContext = new ControllerContext(httpContext, routeData, controller);

            var result = viewEngine.FindPartialView(controllerContext, "index", false);
            return new ViewContext(controllerContext, result.View, new ViewDataDictionary(), new TempDataDictionary());
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

        [Test]
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
