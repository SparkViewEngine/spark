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
            public override Guid GeneratedViewId
            {
                get { throw new NotImplementedException(); }
            }

            public override bool TryGetViewData(string name, out object value)
            {
                throw new NotImplementedException();
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
            var httpContext = mocks.StrictMock<HttpContextBase>();
            var httpRequest = mocks.StrictMock<HttpRequestBase>();
            SetupResult.For(httpContext.Request).Return(httpRequest);

            var controller = mocks.StrictMock<ControllerBase>();

            Expect.Call(httpRequest.ApplicationPath).Return("/");
            Expect.Call(httpRequest.ApplicationPath).Return("/TestApp");
            Expect.Call(httpRequest.ApplicationPath).Return("/TestApp/");
            Expect.Call(httpRequest.ApplicationPath).Return("");
            Expect.Call(httpRequest.ApplicationPath).Return(null);
            Expect.Call(httpRequest.ApplicationPath).Return("TestApp/");
            Expect.Call(httpRequest.ApplicationPath).Return("TestApp");

            mocks.ReplayAll();

            var view = new StubSparkView();
            var viewContext = new ViewContext(new ControllerContext(httpContext, new RouteData(), controller), view, new ViewDataDictionary(), new TempDataDictionary(), new StringWriter());

            view = new StubSparkView { ViewContext = viewContext };
            Assert.That(view.SiteRoot, Is.EqualTo(""));

            view = new StubSparkView { ViewContext = viewContext };
            Assert.That(view.SiteRoot, Is.EqualTo("/TestApp"));

            view = new StubSparkView { ViewContext = viewContext };
            Assert.That(view.SiteRoot, Is.EqualTo("/TestApp"));

            view = new StubSparkView { ViewContext = viewContext };
            Assert.That(view.SiteRoot, Is.EqualTo(""));

            view = new StubSparkView { ViewContext = viewContext };
            Assert.That(view.SiteRoot, Is.EqualTo(""));

            view = new StubSparkView { ViewContext = viewContext };
            Assert.That(view.SiteRoot, Is.EqualTo("/TestApp"));

            view = new StubSparkView { ViewContext = viewContext };
            Assert.That(view.SiteRoot, Is.EqualTo("/TestApp"));

            mocks.VerifyAll();
        }

        private class ModelViewTest : SparkView<string>
        {
            public override void Render()
            {
                throw new NotImplementedException();
            }

            public override Guid GeneratedViewId => throw new NotImplementedException();
        }

        [Test]
        public void CanAccessModelViaModel()
        {
            var view = new ModelViewTest { ViewData = { Model = "asd" } };

            Assert.That(view.Model, Is.EqualTo("asd"));
        }

        [Test, Ignore("Doesn't work for automated build")]
        public void CanAccessViewBagViaView()
        {
            var view = new ModelViewTest { ViewBag = { SomeProperty = "dgf" } };

            Assert.That(view.ViewBag.SomeProperty, Is.EqualTo("dgf"));
        }
    }
}
