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

using Castle.MonoRail.Framework.Providers;

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
        private IResponse response;
        private IServerUtility server;

        private IController controller;
        private IControllerContext controllerContext;

        private IDictionary propertyBag;

        private StringWriter output;
        private DefaultViewEngineManager manager;
        private IRoutingEngine routingEngine;
        private SparkViewFactory factory;
        private StubMonoRailServices serviceProvider;

        [SetUp]
        public void Init()
        {
            mocks = new MockRepository();
            serviceProvider = new StubMonoRailServices();

            var viewSourceLoader = new FileAssemblyViewSourceLoader("MonoRail.Tests.Views");
            viewSourceLoader.Service(this.serviceProvider);
            serviceProvider.ViewSourceLoader = viewSourceLoader;
            serviceProvider.AddService(typeof(IViewSourceLoader), viewSourceLoader);
            
            factory = new SparkViewFactory();
            factory.Service(serviceProvider);

            manager = new DefaultViewEngineManager();
            manager.Service(serviceProvider);
            serviceProvider.ViewEngineManager = manager;
            serviceProvider.AddService(typeof(IViewEngineManager), manager);

            manager.RegisterEngineForExtesionLookup(factory);
            manager.RegisterEngineForView(factory);


            controllerContext = new ControllerContext();
            propertyBag = controllerContext.PropertyBag;
            
            controllerContext.LayoutNames = new []{"default"};
            output = new StringWriter();

            server = new StubServerUtility();
            routingEngine = mocks.CreateMock<IRoutingEngine>();
            var urlBuilder = new DefaultUrlBuilder(server, routingEngine);
            serviceProvider.UrlBuilder = urlBuilder;
            serviceProvider.AddService(typeof(IUrlBuilder), urlBuilder);

            InitUrlInfo("", "home", "index");

            response = engineContext.Response;
        }


        void InitUrlInfo(string areaName, string controllerName, string actionName)
        {
            var urlInfo = new UrlInfo(areaName, controllerName, actionName, "/", "castle");

            engineContext = new StubEngineContext();
            engineContext.AddService(typeof(IUrlBuilder), serviceProvider.UrlBuilder);
            engineContext.CurrentController = controller;
            engineContext.CurrentControllerContext = controllerContext;
            engineContext.Services.ViewEngineManager = serviceProvider.ViewEngineManager;
            output = (StringWriter) engineContext.Response.Output;

            var routeMatch = new RouteMatch();
            controllerContext.RouteMatch = routeMatch;
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
        public void ExtensionIsSpark()
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
            
            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add("Home\\Index.spark");
            descriptor.Templates.Add("Shared\\default.spark");
            var entry = factory.Engine.GetEntry(descriptor);
            var view = (SparkView)entry.CreateInstance();
            view.Contextualize(engineContext, controllerContext, factory);
            
            var result = new StringWriter();
            view.RenderView(result);
            Assert.AreEqual(result.ToString(), output.ToString());
            Assert.AreSame(engineContext, view.Context);
            Assert.AreSame(controllerContext, view.ControllerContext);
        }

        [Test, Ignore("Need to get the helpers to function again using the stub objects")]
        public void HelperModelDictionaries()
        {
            InitUrlInfo("", "Home", "Index");

            mocks.ReplayAll();
            controllerContext.Helpers.Add("FormHelper", new FormHelper(engineContext));
            var urlHelper = new UrlHelper(engineContext);
            controllerContext.Helpers.Add("UrlHelper", urlHelper);
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

        [Test]
        public void Rescue404Rendering()
        {
            //SetupResult.For(response.StatusCode).PropertyBehavior();
            //SetupResult.For(response.StatusDescription).PropertyBehavior();
            mocks.ReplayAll();
            var handler = new MonoRailHttpHandlerFactory.NotFoundHandler("", "nosuchcontroller", engineContext);
            handler.ProcessRequest(null);
            Assert.AreEqual(404, response.StatusCode);
            Assert.AreEqual("<p>404 message rendered</p>", output.ToString());
        }

        [Test]
        public void ControllerHelperAttributeCanBeUsed()
        {
            controller = new Helpers.HomeController();
            controllerContext.ControllerDescriptor = serviceProvider.ControllerDescriptorProvider.BuildDescriptor(controller);
            controllerContext.Helpers.Add("TestingHelper", new Helpers.TestingHelper());
            mocks.ReplayAll();
            manager.Process("Home\\ControllerHelperAttributeCanBeUsed", output, engineContext, controller, controllerContext);
            Assert.That(output.ToString().Contains("<p>Hello</p>"));            
        }
    }

}
    
