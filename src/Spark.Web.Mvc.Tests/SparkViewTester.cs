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
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;

namespace Spark.Web.Mvc.Tests
{
    [TestFixture]
    public class SparkViewTester
    {
        private class StubSparkView : SparkView
        {
            public StubSparkView()
                : base(null)
            {
            }
            public StubSparkView(SparkViewBase<MvcViewContext> decorated)
                : base(decorated)
            {
            }

            public override Guid GeneratedViewId
            {
                get { throw new NotImplementedException(); }
            }

            public override bool TryGetViewData(string name, out object value)
            {
                throw new System.NotImplementedException();
            }

            public override void Render()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void SiteRootActsAsSafePrefix()
        {
            var mocks = new MockRepository();
            var httpContext = mocks.CreateMock<HttpContextBase>();
            var httpRequest = mocks.CreateMock<HttpRequestBase>();
            SetupResult.For(httpContext.Request).Return(httpRequest);

            var controller = mocks.CreateMock<ControllerBase>();

            Expect.Call(httpRequest.ApplicationPath).Return("/");
            Expect.Call(httpRequest.ApplicationPath).Return("/TestApp");
            Expect.Call(httpRequest.ApplicationPath).Return("/TestApp/");
            Expect.Call(httpRequest.ApplicationPath).Return("");
            Expect.Call(httpRequest.ApplicationPath).Return(null);
            Expect.Call(httpRequest.ApplicationPath).Return("TestApp/");
            Expect.Call(httpRequest.ApplicationPath).Return("TestApp");

            mocks.ReplayAll();

            var view = new StubSparkView();
            var viewContext = new ViewContext(httpContext, new RouteData(), controller, view, null, null);

            view = new StubSparkView { ViewContext = viewContext };
            Assert.AreEqual("", view.SiteRoot);

            view = new StubSparkView {ViewContext = viewContext};
            Assert.AreEqual("/TestApp", view.SiteRoot);

            view = new StubSparkView {ViewContext = viewContext};
            Assert.AreEqual("/TestApp", view.SiteRoot);

            view = new StubSparkView {ViewContext = viewContext};
            Assert.AreEqual("", view.SiteRoot);

            view = new StubSparkView {ViewContext = viewContext};
            Assert.AreEqual("", view.SiteRoot);

            view = new StubSparkView {ViewContext = viewContext};
            Assert.AreEqual("/TestApp", view.SiteRoot);

            view = new StubSparkView {ViewContext = viewContext};
            Assert.AreEqual("/TestApp", view.SiteRoot);

            mocks.VerifyAll();
        }

        private class ModelViewTest : SparkView<string>
        {
            public override void Render()
            {
                
            }

            public override Guid GeneratedViewId
            {
                get { throw new System.NotImplementedException(); }
            }
        }

        [Test]
        public void CanAccessModelViaModel()
        {
            var view = new ModelViewTest();
            view.ViewData.Model = "asd";
            Assert.AreEqual("asd", view.Model);
        }
    }
}
