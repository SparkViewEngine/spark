using System.IO;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.Pdf.Tests.Stubs;
using Castle.MonoRail.TestSupport;
using NUnit.Framework;

namespace Castle.MonoRail.Pdf.Tests
{
    [TestFixture]
    public class FormatReturnBinderTests: BaseControllerTest
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

            Assert.AreEqual(200, controller.Context.Response.StatusCode);
            Assert.AreEqual("OK", controller.Context.Response.StatusDescription);
            Assert.AreEqual(string.Format("ReturnBinderTest{0}Index", Path.DirectorySeparatorChar), viewEngineManager.TemplateRendered);
            Assert.AreEqual(0, controller.Context.Response.Output.ToString().Length);
        }

        [Test]
        public void RequestsForJsonAreHandled()
        {
            engineContext.UrlInfo = new UrlInfo(null, "ReturnBinderTest", "Index", null, "json");
            controller.Process(controller.Context, controllerContext);

            Assert.AreEqual(200, controller.Context.Response.StatusCode);
            Assert.AreEqual("OK", controller.Context.Response.StatusDescription);
            Assert.IsNull(viewEngineManager.TemplateRendered);
            Assert.AreEqual("application/json, text/javascript", controller.Context.Response.ContentType);
            Assert.AreEqual("{\"Foo\":\"Bar\"}", controller.Context.Response.Output.ToString());
 
        }

        [Test]
        public void RequestsForPdfAreHandled()
        {
            engineContext.UrlInfo = new UrlInfo(null, "ReturnBinderTest", "Index", null, "pdf");
            viewEngineManager.SetRenderedOutPut("<itext><paragraph>hello</paragraph></itext>");
            controller.Process(controller.Context, controllerContext);

            Assert.AreEqual(200, controller.Context.Response.StatusCode);
            Assert.AreEqual("OK", controller.Context.Response.StatusDescription);
            Assert.AreEqual(string.Format("ReturnBinderTest{0}Index.pdf.spark", Path.DirectorySeparatorChar), viewEngineManager.TemplateRendered);
            Assert.AreEqual("application/pdf", controller.Context.Response.ContentType);
            Assert.Greater(controller.Context.Response.OutputStream.Length, 0);
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