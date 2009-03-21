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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using Spark.FileSystem;
using Spark.Web.Mvc.Tests.Controllers;
using Spark.Web.Mvc.Tests.Models;

namespace Spark.Web.Mvc.Tests
{
    [TestFixture, Category("SparkViewEngine")]
    public class SparkViewFactoryTester
    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
            // clears cache
            CompiledViewHolder.Current = null;

            // reset routes
            RouteTable.Routes.Clear();
            RouteTable.Routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
                                      {
                                          Defaults = new RouteValueDictionary(new { action = "Index", id = "" })
                                      });

            _contextBase = MockRepository.GenerateStub<HttpContextBase>();
            _requestBase = MockRepository.GenerateStub<HttpRequestBase>();
            _responseBase = MockRepository.GenerateStub<HttpResponseBase>();
            _sessionStateBase = MockRepository.GenerateStub<HttpSessionStateBase>();
            _serverUtilityBase = MockRepository.GenerateStub<HttpServerUtilityBase>();

            _contextBase.Stub(x => x.Request).Return(_requestBase);
            _contextBase.Stub(x => x.Response).Return(_responseBase);
            _contextBase.Stub(x => x.Session).Return(_sessionStateBase);
            _contextBase.Stub(x => x.Server).Return(_serverUtilityBase);

            _responseBase.Stub(x => x.OutputStream).Return(new MemoryStream());
            _responseBase.Stub(x => x.Output).Return(new StringWriter());


            _requestBase.Stub(x => x.ApplicationPath).Return("/");
            _requestBase.Stub(x => x.Path).Return("/");
            _responseBase.Stub(x => x.ApplyAppPathModifier(null))
                .IgnoreArguments()
                .Do(new Func<string, string>(path => path));

            context = _contextBase;
            response = context.Response;

            output = response.Output;

            controller = MockRepository.GenerateStub<ControllerBase>();


            routeData = new RouteData();
            routeData.Values.Add("controller", "Home");
            routeData.Values.Add("action", "Index");

            controllerContext = new ControllerContext(context, routeData, controller);

            var settings = new SparkSettings().AddNamespace("System.Web.Mvc.Html");
            factory = new SparkViewFactory(settings) { ViewFolder = new FileSystemViewFolder("AspNetMvc.Tests.Views") };

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(factory);
        }

        #endregion

        private HttpContextBase context;
        private HttpResponseBase response;
        private ControllerBase controller;
        private RouteData routeData;
        private ControllerContext controllerContext;

        private SparkViewFactory factory;

        private TextWriter output;

        private HttpContextBase _contextBase;
        private HttpRequestBase _requestBase;
        private HttpResponseBase _responseBase;
        private HttpSessionStateBase _sessionStateBase;
        private HttpServerUtilityBase _serverUtilityBase;

        private ViewContext MakeViewContext(string viewName)
        {
            var result = factory.FindView(controllerContext, viewName, null);
            return new ViewContext(controllerContext, result.View, new ViewDataDictionary(), new TempDataDictionary());
        }

        private void FindViewAndRender(string viewName)
        {
            FindViewAndRender(viewName, null, null);
        }

        private void FindViewAndRender(string viewName, string masterName)
        {
            FindViewAndRender(viewName, masterName, null);
        }

        private void FindViewAndRender(string viewName, object viewData)
        {
            FindViewAndRender(viewName, null, viewData);
        }

        private void FindViewAndRender(string viewName, string masterName, object viewData)
        {
            var result = factory.FindView(controllerContext, viewName, masterName);
            var viewContext = new ViewContext(controllerContext, result.View, new ViewDataDictionary(viewData), new TempDataDictionary());
            viewContext.View.Render(viewContext, output);
        }

        private class ViewDataContainer : IViewDataContainer
        {
            #region IViewDataContainer Members

            public ViewDataDictionary ViewData { get; set; }

            #endregion
        }

        private static void ContainsInOrder(string content, params string[] values)
        {
            var index = 0;
            foreach (var value in values)
            {
                var nextIndex = content.IndexOf(value, index);
                Assert.GreaterOrEqual(nextIndex, 0, string.Format("Looking for {0}", value));
                index = nextIndex + value.Length;
            }
        }

        [Test]
        public void CaptureNamedContent()
        {

            FindViewAndRender("namedcontent", "layout");

            //mocks.VerifyAll();
            var content = output.ToString();
            Assert.That(content.Contains("<p>main content</p>"));
            Assert.That(content.Contains("<p>this is the header</p>"));
            Assert.That(content.Contains("<p>footer part one</p>"));
            Assert.That(content.Contains("<p>footer part two</p>"));
        }

        [Test]
        public void DeclaringViewDataAccessor()
        {
            
            var comments = new[] { new Comment { Text = "foo" }, new Comment { Text = "bar" } };

            FindViewAndRender("viewdata", new { Comments = comments, Caption = "Hello world" });

            //mocks.VerifyAll();
            var content = output.ToString();
            Assert.That(content.Contains("<h1>Hello world</h1>"));
            Assert.That(content.Contains("<p>foo</p>"));
            Assert.That(content.Contains("<p>bar</p>"));
        }


        [Test]
        public void ForEachTest()
        {
            

            FindViewAndRender("foreach", null);

            //mocks.VerifyAll();

            var content = output.ToString();
            Assert.That(content.Contains(@"<li class=""odd"">1: foo</li>"));
            Assert.That(content.Contains(@"<li class=""even"">2: bar</li>"));
            Assert.That(content.Contains(@"<li class=""odd"">3: baaz</li>"));
        }


        [Test]
        public void GlobalSetTest()
        {
            

            FindViewAndRender("globalset", null);

            //mocks.VerifyAll();

            var content = output.ToString();
            Assert.That(content.Contains("<p>default: Global set test</p>"));
            Assert.That(content.Contains("<p>7==7</p>"));
        }

        [Test]
        public void HtmlEncodeFunctionH()
        {
            
            FindViewAndRender("html-encode-function-h");
            //mocks.VerifyAll();

            var content = output.ToString().Replace(" ", "").Replace("\r", "").Replace("\n", "");
            Assert.AreEqual("<p>&lt;p&gt;&amp;lt;&amp;gt;&lt;/p&gt;</p>", content);
        }

        [Test]
        public void HtmlHelperWorksOnItsOwn()
        {
            


            var viewContext = MakeViewContext("helpers");

            var html = new HtmlHelper(viewContext, new ViewDataContainer { ViewData = viewContext.ViewData });
            var link = html.ActionLink("hello", "world");
            response.Write(link);

            //mocks.VerifyAll();

            Assert.AreEqual("<a href=\"/Home/world\">hello</a>", link);
        }

        [Test]
        public void MasterApplicationIfPresent()
        {
            var viewFolder = MockRepository.GenerateMock<IViewFolder>();
            viewFolder.Expect(x=>x.HasView("Foo\\Baaz.spark")).Return(true);
            viewFolder.Expect(x=>x.HasView("Layouts\\Foo.spark")).Return(false);
            viewFolder.Expect(x=>x.HasView("Shared\\Foo.spark")).Return(false);
            viewFolder.Expect(x=>x.HasView("Layouts\\Application.spark")).Return(false);
            viewFolder.Expect(x=>x.HasView("Shared\\Application.spark")).Return(true);

            factory.ViewFolder = viewFolder;


            

            routeData.Values["controller"] = "Foo";
            routeData.Values["action"] = "NotBaaz";

            var descriptor = factory.CreateDescriptor(controllerContext, "Baaz", null, true, null);

            //mocks.VerifyAll();

            Assert.AreEqual(2, descriptor.Templates.Count);
            Assert.AreEqual("Foo\\Baaz.spark", descriptor.Templates[0]);
            Assert.AreEqual("Shared\\Application.spark", descriptor.Templates[1]);
        }

        [Test]
        public void MasterEmptyByDefault()
        {
            var viewSourceLoader = MockRepository.GenerateMock<IViewFolder>();
            viewSourceLoader.Expect(x=>x.HasView("Foo\\Baaz.spark")).Return(true);
            viewSourceLoader.Expect(x=>x.HasView("Layouts\\Foo.spark")).Return(false);
            viewSourceLoader.Expect(x=>x.HasView("Shared\\Foo.spark")).Return(false);
            viewSourceLoader.Expect(x=>x.HasView("Layouts\\Application.spark")).Return(false);
            viewSourceLoader.Expect(x=>x.HasView("Shared\\Application.spark")).Return(false);

            factory.ViewFolder = viewSourceLoader;


            

            routeData.Values["controller"] = "Foo";
            routeData.Values["action"] = "NotBaaz";


            var descriptor = factory.CreateDescriptor(controllerContext, "Baaz", null, true, null);

            //mocks.VerifyAll();

            Assert.AreEqual(1, descriptor.Templates.Count);
            Assert.AreEqual("Foo\\Baaz.spark", descriptor.Templates[0]);
        }

        [Test]
        public void MasterForControllerIfPresent()
        {
            var viewSourceLoader = MockRepository.GenerateMock<IViewFolder>();
            viewSourceLoader.Expect(x=>x.HasView("Foo\\Baaz.spark")).Return(true);
            viewSourceLoader.Expect(x=>x.HasView("Layouts\\Foo.spark")).Return(false);
            viewSourceLoader.Expect(x=>x.HasView("Shared\\Foo.spark")).Return(true);

            factory.ViewFolder = viewSourceLoader;


            

            routeData.Values["controller"] = "Foo";
            routeData.Values["action"] = "NotBaaz";

            var descriptor = factory.CreateDescriptor(controllerContext, "Baaz", null, true, null);

            //mocks.VerifyAll();

            Assert.AreEqual(2, descriptor.Templates.Count);
            Assert.AreEqual("Foo\\Baaz.spark", descriptor.Templates[0]);
            Assert.AreEqual("Shared\\Foo.spark", descriptor.Templates[1]);
        }

        [Test]
        public void MasterTest()
        {
            

            FindViewAndRender("childview", "layout");

            //mocks.VerifyAll();
            var content = output.ToString();
            Assert.That(content.Contains("<title>Standalone Index View</title>"));
            Assert.That(content.Contains("<h1>Standalone Index View</h1>"));
            Assert.That(content.Contains("<p>no header by default</p>"));
            Assert.That(content.Contains("<p>no footer by default</p>"));
        }

        [Test]
        public void NullViewDataIsSafe()
        {
            
            FindViewAndRender("viewdatanull", null);
            //mocks.VerifyAll();

            var content = output.ToString();
            Assert.That(content.Contains("<p>nothing</p>"));
        }

        [Test]
        public void RenderPartialOrderCorrect()
        {
            
            FindViewAndRender("renderpartial-ordercorrect", "ajax");
            //mocks.VerifyAll();

            var content = output.ToString();
            ContainsInOrder(content,
                            "<p>one</p>",
                            "<p>two</p>",
                            "<p>three</p>");
        }

        [Test]
        public void RenderPlainView()
        {
            

            var viewContext = MakeViewContext("index");
            var viewEngineResult = factory.FindView(controllerContext, "index", null);
            viewEngineResult.View.Render(viewContext, output);

            //mocks.VerifyAll();
        }

        [Test]
        public void TargetNamespaceFromController()
        {
            var viewSourceLoader = MockRepository.GenerateMock<IViewFolder>();
            viewSourceLoader.Expect(x=>x.HasView("Home\\Baaz.spark")).Return(true);
            viewSourceLoader.Expect(x=>x.HasView("Layouts\\Home.spark")).Return(true);
            factory.ViewFolder = viewSourceLoader;

            controller = new StubController();
            controllerContext = new ControllerContext(context, routeData, controller);

            

            var descriptor = factory.CreateDescriptor(controllerContext, "Baaz", null, true, null);
            //mocks.VerifyAll();

            Assert.AreEqual("Spark.Web.Mvc.Tests.Controllers", descriptor.TargetNamespace);
        }


        [Test]
        public void UsingHtmlHelper()
        {
            

            FindViewAndRender("helpers", null);

            //mocks.VerifyAll();
            var content = output.ToString();
            Assert.That(content.Contains("<p><a href=\"/Home/Sort\">Click me</a></p>"));
            Assert.That(content.Contains("<p>foo&gt;bar</p>"));
        }

        [Test]
        public void UsingNamespace()
        {
            
            FindViewAndRender("usingnamespace");

            //mocks.VerifyAll();
            var content = output.ToString();
            Assert.That(content.Contains("<p>Foo</p>"));
            Assert.That(content.Contains("<p>Bar</p>"));
            Assert.That(content.Contains("<p>Hello</p>"));
        }

        [Test]
        public void UsingPartialFile()
        {
            

            FindViewAndRender("usingpartial", null);

            //mocks.VerifyAll();
            var content = output.ToString();
            Assert.That(content.Contains("<li>Partial where x=\"zero\"</li>"));
            Assert.That(content.Contains("<li>Partial where x=\"one\"</li>"));
            Assert.That(content.Contains("<li>Partial where x=\"two\"</li>"));
            Assert.That(content.Contains("<li>Partial where x=\"three\"</li>"));
            Assert.That(content.Contains("<li>Partial where x=\"four\"</li>"));
        }

        [Test]
        public void UsingPartialFileImplicit()
        {
            

            FindViewAndRender("usingpartialimplicit", null);

            //mocks.VerifyAll();
            var content = output.ToString();
            Assert.That(content.Contains("<li class=\"odd\">one</li>"));
            Assert.That(content.Contains("<li class=\"even\">two</li>"));
        }


        [Test]
        public void ViewDataWithModel()
        {
            
            FindViewAndRender("viewdatamodel", new Comment { Text = "Hello" });
            //mocks.VerifyAll();

            var content = output.ToString();
            Assert.That(content.Contains("<p>Hello</p>"));
        }

        [Test]
        public void ViewSourceLoaderCanBeChanged()
        {
            var replacement = MockRepository.GenerateStub<IViewFolder>();

            

            var existing = factory.ViewFolder;
            Assert.AreNotSame(existing, replacement);
            Assert.AreSame(existing, factory.ViewFolder);

            factory.ViewFolder = replacement;
            Assert.AreSame(replacement, factory.ViewFolder);
            Assert.AreNotSame(existing, factory.ViewFolder);
        }

        [Test]
        public void CreatingViewEngineWithSimpleContainer()
        {
            var settings = new SparkSettings().AddNamespace("System.Web.Mvc.Html");
            var container = new SparkServiceContainer(settings);
            container.SetServiceBuilder<IViewEngine>(c => new SparkViewFactory(c.GetService<ISparkSettings>()));

            var viewFactory = (SparkViewFactory)container.GetService<IViewEngine>();
            var viewEngine = container.GetService<ISparkViewEngine>();
            var viewFolder = container.GetService<IViewFolder>();
            Assert.AreSame(settings, viewFactory.Settings);
            Assert.AreSame(settings, viewEngine.Settings);
            Assert.AreSame(viewEngine, viewFactory.Engine);
            Assert.AreSame(viewFolder, viewEngine.ViewFolder);
            Assert.AreSame(viewFolder, viewFactory.ViewFolder);
        }


        [Test]
        public void EvalWithViewDataModel()
        {
            
            FindViewAndRender("EvalWithViewDataModel", new Comment { Text = "Hello" });
            //mocks.VerifyAll();

            var content = output.ToString();
            Assert.That(content.Contains("<p>Hello</p>"));
        }

        [Test]
        public void EvalWithAnonModel()
        {
            
            FindViewAndRender("EvalWithAnonModel", new { Foo = 42, Bar = new Comment { Text = "Hello" } });
            //mocks.VerifyAll();

            var content = output.ToString();
            Assert.That(content.Contains("<p>42 Hello</p>"));
        }


        public class ScopedCulture:IDisposable
        {
            private readonly CultureInfo savedCulture;
 
            public ScopedCulture(CultureInfo culture)
            {
                savedCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = culture;
            }

            public void Dispose()
            {
                Thread.CurrentThread.CurrentCulture = savedCulture;
            }
        }

        [Test]
        public void EvalWithFormatString()
        {
            using ( new ScopedCulture( CultureInfo.CreateSpecificCulture("en-us") ) )
            {
                mocks.ReplayAll();
                FindViewAndRender("EvalWithFormatString", new { Cost = 134567.89, terms = new { due = new DateTime(1971, 10, 14) } });

                var content = output.ToString();
                Assert.That(content.Contains("<p>134,567.89</p>"));
                Assert.That(content.Contains("<p>1971/10/14</p>"));
            }
        }

        [Test]
        public void RenderPartialSharesState()
        {
            
            FindViewAndRender("RenderPartialSharesState");
            //mocks.VerifyAll();

            var content = output.ToString();
            ContainsInOrder(content,
                            "<p>before</p>",
                            "<p>foo1</p>",
                            "<p>bar3</p>",
                            "<p>The Target</p>",
                            "<p>quux6</p>",
                            "<p>after</p>",
                            "<ul>",
                            "<li>one</li>",
                            "<li>three</li>",
                            "<li>two</li>",
                            "</ul>",
                            "alphabetagammadelta");
            Assert.IsFalse(content.Contains("foo2"));
            Assert.IsFalse(content.Contains("bar4"));
            Assert.IsFalse(content.Contains("quux7"));
        }

        [Test]
        public void CanLocateViewInArea()
        {
            
            controllerContext.RouteData.Values.Add("area", "admin");
            var result = factory.FindView(controllerContext, "index", null);
            var viewContext = new ViewContext(controllerContext, result.View, new ViewDataDictionary(), new TempDataDictionary());
            viewContext.View.Render(viewContext, output);
            //mocks.VerifyAll();

            Assert.AreEqual("<p>default view admin area</p>", output.ToString().Trim());
        }

        [Test]
        public void CanLocateViewInAreaWithLayout()
        {
            
            controllerContext.RouteData.Values.Add("area", "admin");
            var result = factory.FindView(controllerContext, "index", "layout");
            var viewContext = new ViewContext(controllerContext, result.View, new ViewDataDictionary(), new TempDataDictionary());
            viewContext.View.Render(viewContext, output);
            //mocks.VerifyAll();

            ContainsInOrder(output.ToString(),
                            "<body>",
                            "<p>default view admin area</p>",
                            "</body>");
        }

        [Test]
        public void CanLocateViewInAreaWithLayoutInArea()
        {
            
            controllerContext.RouteData.Values.Add("area", "admin");
            var result = factory.FindView(controllerContext, "index", "speciallayout");
            var viewContext = new ViewContext(controllerContext, result.View, new ViewDataDictionary(), new TempDataDictionary());
            viewContext.View.Render(viewContext, output);
            //mocks.VerifyAll();

            ContainsInOrder(output.ToString(),
                            "<body class=\"special\">",
                            "<p>default view admin area</p>",
                            "</body>");
        }

        [Test]
        public void CanLocatePartialViewInArea()
        {
            
            controllerContext.RouteData.Values.Add("area", "admin");
            var result = factory.FindPartialView(controllerContext, "index");
            var viewContext = new ViewContext(controllerContext, result.View, new ViewDataDictionary(), new TempDataDictionary());
            viewContext.View.Render(viewContext, output);
            //mocks.VerifyAll();

            Assert.AreEqual("<p>default view admin area</p>", output.ToString().Trim());
        }
    }
}
