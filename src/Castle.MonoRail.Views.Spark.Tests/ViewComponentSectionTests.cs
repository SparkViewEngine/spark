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
    using System.IO;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Services;
    using Castle.MonoRail.Framework.Test;
    using NUnit.Framework;
    using Rhino.Mocks;

    [TestFixture]
    public class ViewComponentSectionTests
    {
        private DefaultViewComponentFactory viewComponentFactory;
        private MockRepository mocks;
        private ControllerContext controllerContext;
        private StubEngineContext engineContext;
        private SparkViewFactory factory;
        private IController controller;

        [SetUp]
        public void Init()
        {
            mocks = new MockRepository();

            var services = new StubMonoRailServices();
            services.ViewSourceLoader = new FileAssemblyViewSourceLoader("MonoRail.Tests.Views");
            services.AddService(typeof(IViewSourceLoader), services.ViewSourceLoader);

            viewComponentFactory = new DefaultViewComponentFactory();
            viewComponentFactory.Initialize();
            services.AddService(typeof(IViewComponentFactory), viewComponentFactory);
            services.AddService(typeof(IViewComponentRegistry), viewComponentFactory.Registry);

            viewComponentFactory.Registry.AddViewComponent("ComponentWithSections", typeof(ComponentWithSections));

            factory = new SparkViewFactory();
            factory.Service(services);

            controller = mocks.CreateMock<IController>();
            controllerContext = new ControllerContext();
            engineContext = new StubEngineContext(new UrlInfo("", "Home", "Index", "/", "castle"));
            engineContext.AddService(typeof(IViewComponentFactory), viewComponentFactory);
            engineContext.AddService(typeof(IViewComponentRegistry), viewComponentFactory.Registry);
        }

        [Test]
        public void ComponentWithSimpleSections()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process("Home\\ComponentWithSimpleSections.spark", writer, engineContext, controller, controllerContext);

            var output = writer.ToString();
            Assert.IsTrue(output.Contains("this-is-a-header"));
            Assert.IsTrue(output.Contains("this-is-a-body"));
            Assert.IsTrue(output.Contains("this-is-a-footer"));
        }

        [Test]
        public void ComponentWithIfConditionInSection()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process("Home\\ComponentWithComplexSections.spark", writer, engineContext, controller, controllerContext);

            var output = writer.ToString();
            Assert.IsTrue(output.Contains("this-should-show-up"));
            Assert.IsFalse(output.Contains("this-should-not-show-up"));
            Assert.IsFalse(output.Contains("if condition"));
        }

        [Test]
        public void ComponentWithForEachInSection()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process("Home\\ComponentWithComplexSections.spark", writer, engineContext, controller, controllerContext);

            var output = writer.ToString();
            Assert.IsTrue(output.Contains("1,2,3,"));
            Assert.IsTrue(output.Contains("<span>10</span><span>9</span><span>8</span>"));
            Assert.IsFalse(output.Contains("for each"));
            Assert.IsFalse(output.Contains("span each"));
        }
    }

    [ViewComponentDetails("ComponentWithSections")]
    class ComponentWithSections : ViewComponent
    {
        public override void Render()
        {
            RenderSection("header");
            RenderSection("body");
            RenderSection("footer");
        }
    }
}
