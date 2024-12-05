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
namespace Castle.MonoRail.Views.Spark.Tests
{
    using System.Collections;
    using System.IO;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Routing;
    using Castle.MonoRail.Framework.Services;
    using Castle.MonoRail.Framework.Test;
    using NUnit.Framework;
    using Rhino.Mocks;

    public abstract class SparkViewFactoryTestsBase
    {
        protected MockRepository mocks;
        protected IEngineContext engineContext;
        protected IResponse response;
        private IServerUtility server;
        protected IController controller;
        protected IControllerContext controllerContext;
        protected IDictionary propertyBag;
        protected StringWriter output;
        protected DefaultViewEngineManager manager;
        private IRoutingEngine routingEngine;
        protected SparkViewFactory factory;
        protected StubMonoRailServices serviceProvider;

        [SetUp]
        public void Init()
        {
            mocks = new MockRepository();
            serviceProvider = new StubMonoRailServices();

            var viewSourceLoader = new FileAssemblyViewSourceLoader("MonoRail.Tests.Views");
            viewSourceLoader.Service(this.serviceProvider);
            serviceProvider.ViewSourceLoader = viewSourceLoader;
            serviceProvider.AddService(typeof(IViewSourceLoader), viewSourceLoader);

            Configure();

            controllerContext = new ControllerContext();
            propertyBag = controllerContext.PropertyBag;

            controllerContext.LayoutNames = new[] { "default" };
            output = new StringWriter();

            server = new StubServerUtility();
            routingEngine = MockRepository.GenerateMock<IRoutingEngine>();
            var urlBuilder = new DefaultUrlBuilder(server, routingEngine);
            serviceProvider.UrlBuilder = urlBuilder;
            serviceProvider.AddService(typeof(IUrlBuilder), urlBuilder);

            InitUrlInfo("", "home", "index");

            response = engineContext.Response;
        }

        [TearDown]
        public void TearDown()
        {
            output.Dispose();
        }

        protected abstract void Configure();

        protected void InitUrlInfo(string areaName, string controllerName, string actionName)
        {
            _ = new UrlInfo(areaName, controllerName, actionName, "/", "castle");

            engineContext = new StubEngineContext();
            engineContext.AddService(typeof(IUrlBuilder), serviceProvider.UrlBuilder);
            engineContext.CurrentController = controller;
            engineContext.CurrentControllerContext = controllerContext;
            engineContext.Services.ViewEngineManager = serviceProvider.ViewEngineManager;
            output = (StringWriter)engineContext.Response.Output;

            var routeMatch = new RouteMatch();
            controllerContext.RouteMatch = routeMatch;
        }


        protected static void ContainsInOrder(string content, params string[] values)
        {
            int index = 0;
            foreach (string value in values)
            {
                int nextIndex = content.IndexOf(value, index);

                Assert.That(nextIndex, Is.GreaterThanOrEqualTo(0), () => $"Looking for {value}");
                index = nextIndex + value.Length;
            }
        }
    }
}