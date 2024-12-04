using System.IO;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.Pdf.Tests.Stubs;
using Castle.MonoRail.TestSupport;
using NUnit.Framework;

namespace Castle.MonoRail.Pdf.Tests
{
    [TestFixture]
    public class FormatReturnBinderTests : BaseControllerTest
    {
        protected Controller controller;
        protected StubMonoRailServices services;
        protected InjectableStubViewEngineManager viewEngineManager;
        protected StubEngineContext engineContext;
        protected IControllerContext controllerContext;

        [Test]
        public void RequestsForNonHandledExtensionsAreIgnored()
        {
            engineContext.UrlInfo = new UrlInfo(null, "ReturnBinderTest", "Index", null, "rails");
            controller.Process(controller.Context, controllerContext);

            Assert.That(controller.Context.Response.StatusCode, Is.EqualTo(200));
            Assert.That(controller.Context.Response.StatusDescription, Is.EqualTo("OK"));
            Assert.That(viewEngineManager.TemplateRendered, Is.EqualTo(string.Format("ReturnBinderTest{0}Index", Path.DirectorySeparatorChar)));
            Assert.That(controller.Context.Response.Output.ToString().Length, Is.EqualTo(0));
        }

        [Test]
        public void RequestsForJsonAreHandled()
        {
            engineContext.UrlInfo = new UrlInfo(null, "ReturnBinderTest", "Index", null, "json");
            controller.Process(controller.Context, controllerContext);

            Assert.That(controller.Context.Response.StatusCode, Is.EqualTo(200));
            Assert.That(controller.Context.Response.StatusDescription, Is.EqualTo("OK"));
            Assert.That(viewEngineManager.TemplateRendered, Is.Null);
            Assert.That(controller.Context.Response.ContentType, Is.EqualTo("application/json, text/javascript"));
            Assert.That(controller.Context.Response.Output.ToString(), Is.EqualTo("{\"Foo\":\"Bar\"}"));

        }

        [Test]
        public void RequestsForPdfAreHandled()
        {
            engineContext.UrlInfo = new UrlInfo(null, "ReturnBinderTest", "Index", null, "pdf");
            viewEngineManager.SetRenderedOutPut("<itext><paragraph>hello</paragraph></itext>");
            controller.Process(controller.Context, controllerContext);

            Assert.That(controller.Context.Response.StatusCode, Is.EqualTo(200));
            Assert.That(controller.Context.Response.StatusDescription, Is.EqualTo("OK"));
            Assert.That(viewEngineManager.TemplateRendered, Is.EqualTo(string.Format("ReturnBinderTest{0}Index.pdf.spark", Path.DirectorySeparatorChar)));
            Assert.That(controller.Context.Response.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(controller.Context.Response.OutputStream.Length, Is.GreaterThan(0));
        }

        [SetUp]
        public void SetUp()
        {
            controller = new ReturnBinderTestController();
            viewEngineManager = new InjectableStubViewEngineManager();
            PrepareController(controller);

            services = controller.Context.Services as StubMonoRailServices;
            services.ViewEngineManager = viewEngineManager;

            engineContext = controller.Context as StubEngineContext;
            engineContext.Request.Headers.Add("User-Agent", "Test Fixture");

            controllerContext = services.ControllerContextFactory.
                Create("", "ReturnBinderTest", "Index", services.ControllerDescriptorProvider.BuildDescriptor(controller));

            controller.Contextualize(controller.Context, controllerContext);
            viewEngineManager.RegisterTemplate(string.Format("ReturnBinderTest{0}Index", Path.DirectorySeparatorChar));
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }
    }

    public class SampleViewModel
    {
        public string Foo { get; set; }
    }

    public class ReturnBinderTestController : SmartDispatcherController
    {
        public ReturnBinderTestController()
        {

        }
        [return: FormatReturnBinder]
        public SampleViewModel Index()
        {
            return new SampleViewModel { Foo = "Bar" };
        }
    }
}