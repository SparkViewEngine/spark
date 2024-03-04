using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Rhino.Mocks;
using Spark.FileSystem;
using Spark.Web.Mvc.Extensions;
using Spark.Web.Mvc.Tests;

namespace Spark.Web.Mvc.Pdf.Tests
{
    [TestFixture]
    public class PdfViewResultTests
    {
        private static readonly string HelloWorldXml = @"
<itext creationdate=""3/24/2009 5:49:07 PM"" producer=""Spark.Reporting"">
    <paragraph leading=""18.0"" font=""unknown"" align=""Default"">
        Hello World
        <Chunk once=""test"">${System.DateTime.Now.ToShortDateString()}</Chunk>
    </paragraph>
</itext>
";

        [Test]
        public void PdfResultShouldFindPartialViewAndRenderIt()
        {
            var stream = new MemoryStream();
            var controllerContext = GetControllerContext(stream);

            IView view;
            var viewEngine = MockViewEngine(controllerContext, out view);            

            var result = new PdfViewResult
                         {
                             ViewName = "quux",
                             ViewEngineCollection = new ViewEngineCollection(new[] { viewEngine })
                         };

            result.ExecuteResult(controllerContext);

            viewEngine.VerifyAllExpectations();
            view.VerifyAllExpectations();
        }

        private static IViewEngine MockViewEngine(ControllerContext controllerContext, out IView view)
        {
            var viewEngine = MockRepository.GenerateMock<IViewEngine>();
            view = MockRepository.GenerateMock<IView>();
            
            viewEngine
                .Expect(x => x.FindView(controllerContext, "quux", "", true))
                .Return(new ViewEngineResult(view, viewEngine));

            view
                .Expect(x => x.Render(null, null))
                .IgnoreArguments()
                .Do(new Action<ViewContext, TextWriter>((a, b) => b.Write("<itext><paragraph>hello</paragraph></itext>")));
            return viewEngine;
        }

        [Test]
        public void PdfResultShouldWriteToOutputStream()
        {
            var settings = new SparkSettings();

            var sp = new ServiceCollection()
                .AddSpark(settings)
                .AddSingleton<IViewFolder>(
                    new InMemoryViewFolder
                    {
                        {
                            "foo/bar.spark",
                            HelloWorldXml
                        }
                    })
                .BuildServiceProvider();

            var factory = sp.GetService<SparkViewFactory>();

            var stream = new MemoryStream();
            var controllerContext = GetControllerContext(stream);

            var result = new PdfViewResult
                         {
                             ViewEngineCollection = new ViewEngineCollection(new[] { factory })
                         };

            result.ExecuteResult(controllerContext);

            Assert.That(stream.Length, Is.Not.EqualTo(0));
        }

        [Test]
        public void ContentTypeShouldBeApplicationPdf()
        {
            var stream = new MemoryStream();
            var controllerContext = GetControllerContext(stream);

            IView view;
            var viewEngine = MockViewEngine(controllerContext, out view);


            var result = new PdfViewResult
            {
                ViewName = "quux",
                ViewEngineCollection = new ViewEngineCollection(new[] { viewEngine })
            };

            result.ExecuteResult(controllerContext);

            Assert.That(controllerContext.HttpContext.Response.ContentType, Is.EqualTo("application/pdf"));
        }

        private static ControllerContext GetControllerContext(Stream stream)
        {
            return new ControllerContext(
                MockHttpContextBase.Generate("/", stream),
                GetRouteData(),
                MockRepository.GenerateStub<ControllerBase>());
        }

        private static RouteData GetRouteData()
        {
            var routeData = new RouteData();
            routeData.Values.Add("controller", "foo");
            routeData.Values.Add("action", "bar");
            return routeData;
        }
    }
}
