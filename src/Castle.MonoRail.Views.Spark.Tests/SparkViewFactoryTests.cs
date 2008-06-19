// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Views.Spark.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;

    using Castle.Core.Logging;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Helpers;
    using Castle.MonoRail.Framework.Routing;
    using Castle.MonoRail.Framework.Services;
    using Castle.MonoRail.Framework.Test;

    using NUnit.Framework;
    using Rhino.Mocks;
    using global::Spark;

    [TestFixture]
    public class SparkViewFactoryTests
    {
        private MockRepository mocks;
        private IEngineContext engineContext;
        private IRequest request;
        private IResponse response;
        private IServerUtility server;

        private IController controller;
        private IControllerContext controllerContext;

        private IDictionary propertyBag;
        private Flash flash;
        private IDictionary session;

        private StringWriter output;
        private DefaultViewEngineManager manager;
        private HelperDictionary helpers;
        private IRoutingEngine routingEngine;
        private SparkViewFactory factory;
        private NameValueCollection requestParams;
        private IDictionary contextItems;

        [SetUp]
        public void Init()
        {
            mocks = new MockRepository();
            factory = new SparkViewFactory();
            engineContext = mocks.CreateMock<IEngineContext>();
            server = new MockServerUtility();
            request = mocks.CreateMock<IRequest>();
            response = mocks.CreateMock<IResponse>();

            controller = mocks.CreateMock<IController>();
            controllerContext = mocks.CreateMock<IControllerContext>();
            routingEngine = mocks.CreateMock<IRoutingEngine>();
            output = new StringWriter();
            helpers = new HelperDictionary();

            propertyBag = new Dictionary<string, object>();
            flash = new Flash();
            session = new Dictionary<string, object>();
            requestParams = new NameValueCollection();
            contextItems = new Dictionary<string, object>();


            SetupResult.For(engineContext.Server).Return(server);
            SetupResult.For(engineContext.Request).Return(request);
            SetupResult.For(engineContext.Response).Return(response);
            SetupResult.For(engineContext.CurrentController).Return(controller);
            SetupResult.For(engineContext.CurrentControllerContext).Return(controllerContext);
            SetupResult.For(engineContext.Flash).Return(flash);
            SetupResult.For(engineContext.Session).Return(session);
            SetupResult.For(engineContext.Items).Return(contextItems);

            SetupResult.For(request.Params).Return(requestParams);

            SetupResult.For(controllerContext.LayoutNames).Return(new[] { "default" });
            SetupResult.For(controllerContext.Helpers).Return(helpers);
            SetupResult.For(controllerContext.PropertyBag).Return(propertyBag);

            SetupResult.For(routingEngine.IsEmpty).Return(true);

            var urlBuilder = new DefaultUrlBuilder(server, routingEngine);

            var serviceProvider = mocks.CreateMock<IServiceProvider>();
            var viewSourceLoader = new FileAssemblyViewSourceLoader("Views");
            SetupResult.For(serviceProvider.GetService(typeof(IViewSourceLoader))).Return(viewSourceLoader);
            SetupResult.For(serviceProvider.GetService(typeof(ILoggerFactory))).Return(new NullLogFactory());
            SetupResult.For(serviceProvider.GetService(typeof(ISparkViewEngine))).Return(null);
            SetupResult.For(serviceProvider.GetService(typeof(IUrlBuilder))).Return(urlBuilder);
            SetupResult.For(serviceProvider.GetService(typeof(IViewComponentFactory))).Return(null);
            mocks.Replay(serviceProvider);

            SetupResult.For(engineContext.GetService(null)).IgnoreArguments().Do(
                new Func<Type, object>(serviceProvider.GetService));

            factory.Service(serviceProvider);


            manager = new DefaultViewEngineManager();
            manager.RegisterEngineForExtesionLookup(factory);
            manager.RegisterEngineForView(factory);
        }


        void InitUrlInfo(string areaName, string controllerName, string actionName)
        {
            var urlInfo = new UrlInfo(areaName, controllerName, actionName, "/", "castle");
            SetupResult.For(engineContext.UrlInfo).Return(urlInfo);

            var routeMatch = new RouteMatch();
            SetupResult.For(controllerContext.RouteMatch).Return(routeMatch);
        }

        static void ContainsInOrder(string content, params string[] values)
        {
            int index = 0;
            foreach (string value in values)
            {
                int nextIndex = content.IndexOf(value, index);
                Assert.GreaterOrEqual(nextIndex, 0, string.Format("Looking for {0}", value));
                index = nextIndex + value.Length;
            }
        }

        [Test]
        public void ExtensionIsXml()
        {
            mocks.ReplayAll();
            Assert.AreEqual("spark", factory.ViewFileExtension);
        }

        [Test]
        public void ProcessBasicTemplate()
        {
            mocks.ReplayAll();
            manager.Process("Home\\Index", output, engineContext, controller, controllerContext);
            Assert.That(output.ToString().Contains("<h1>Simple test</h1>"));
        }

        [Test]
        public void ContextAndControllerContextAvailable()
        {
            mocks.ReplayAll();
            manager.Process("Home\\Index", output, engineContext, controller, controllerContext);
            var entry = factory.Engine.GetEntry("Home", "Index", "default");
            var view = (SparkView)entry.CreateInstance();
            view.Contextualize(engineContext, controllerContext, factory);
            
            StringWriter result = new StringWriter();
            view.RenderView(result);
            Assert.AreEqual(result.ToString(), output.ToString());
            Assert.AreSame(engineContext, view.Context);
            Assert.AreSame(controllerContext, view.ControllerContext);
        }

        [Test]
        public void HelperModelDictionaries()
        {
            InitUrlInfo("", "Home", "Index");

            mocks.ReplayAll();
            helpers.Add(new FormHelper(engineContext));
            helpers.Add(new UrlHelper(engineContext));
            manager.Process("Home\\HelperModelDictionaries", output, engineContext, controller, controllerContext);
            ContainsInOrder(output.ToString(),
                            "Home/foo.castle",
                            "<form", "action='/Home/save.castle'", "method='get'",
                            "<input", "type=\"text\"", "id=\"hello\"", "class=\"world\"", "/>",
                            "</form>");
        }


        [Test]
        public void PropertyBagViewdata()
        {
            mocks.ReplayAll();
            propertyBag["foo"] = "baaz";
            propertyBag["bar"] = 7;
            manager.Process("Home\\PropertyBagViewdata", output, engineContext, controller, controllerContext);
            ContainsInOrder(output.ToString(),
                            "<p>foo:baaz</p>",
                            "<p>bar:7</p>",
                            "<p>bar+4:11</p>");
        }

        [Test]
        public void TerseHtmlEncode()
        {
            mocks.ReplayAll();
            manager.Process("Home\\TerseHtmlEncode", output, engineContext, controller, controllerContext);
            ContainsInOrder(output.ToString(),
                "<p>This &lt;contains/&gt; html</p>");
        }

        [Test]
        public void IncludingStatementsDirectly()
        {
            mocks.ReplayAll();
            manager.Process("Home\\CodeStatements", output, engineContext, controller, controllerContext);
            ContainsInOrder(output.ToString(),
                "<p>was true</p>");


            Assert.IsFalse(output.ToString().Contains("<p>was false</p>"));
            
        }
    }

}
