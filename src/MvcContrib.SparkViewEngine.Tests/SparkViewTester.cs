using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;

namespace MvcContrib.SparkViewEngine.Tests
{
    [TestFixture]
    public class SparkViewTester
    {
        class StubSparkView : SparkView
        {
            public override void RenderView(TextWriter writer)
            {
                throw new System.NotImplementedException();
            }

            public override Guid GeneratedViewId
            {
                get { throw new System.NotImplementedException(); }
            }
        }

        [Test]
        public void SiteRootActsAsSafePrefix()
        {
            var mocks = new MockRepository();
            var httpContext = mocks.CreateMock<HttpContextBase>();
            var httpRequest = mocks.CreateMock<HttpRequestBase>();
            SetupResult.For(httpContext.Request).Return(httpRequest);

            var controller = mocks.CreateMock<IController>();

            Expect.Call(httpRequest.ApplicationPath).Return("/");
            Expect.Call(httpRequest.ApplicationPath).Return("/TestApp");
            Expect.Call(httpRequest.ApplicationPath).Return("/TestApp/");
            Expect.Call(httpRequest.ApplicationPath).Return("");
            Expect.Call(httpRequest.ApplicationPath).Return(null);
            Expect.Call(httpRequest.ApplicationPath).Return("TestApp/");
            Expect.Call(httpRequest.ApplicationPath).Return("TestApp");

            mocks.ReplayAll();

            var viewContext = new ViewContext(httpContext, new RouteData(), controller, "index", null, null, null);

            var view = new StubSparkView {ViewContext = viewContext};
            Assert.AreEqual("", view.SiteRoot);
            
            view = new StubSparkView { ViewContext = viewContext };
            Assert.AreEqual("/TestApp", view.SiteRoot);
            
            view = new StubSparkView { ViewContext = viewContext };
            Assert.AreEqual("/TestApp", view.SiteRoot);
            
            view = new StubSparkView { ViewContext = viewContext };
            Assert.AreEqual("", view.SiteRoot);
            
            view = new StubSparkView { ViewContext = viewContext };
            Assert.AreEqual("", view.SiteRoot);
            
            view = new StubSparkView { ViewContext = viewContext };
            Assert.AreEqual("/TestApp", view.SiteRoot);
            
            view = new StubSparkView { ViewContext = viewContext };
            Assert.AreEqual("/TestApp", view.SiteRoot);
            
            mocks.VerifyAll();
        }
    }
}
