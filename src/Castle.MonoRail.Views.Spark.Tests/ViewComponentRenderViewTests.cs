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
    public class ViewComponentRenderViewTests 
    {
        private DefaultViewComponentFactory viewComponentFactory;
        private MockRepository mocks;
        private ControllerContext controllerContext;
        private MockEngineContext engineContext;
        private SparkViewFactory factory;
        private IController controller;

        [SetUp]
        public void Init()
        {
            mocks = new MockRepository();

            var services = new MockServices();
            services.ViewSourceLoader = new FileAssemblyViewSourceLoader("Views");
            services.AddService(typeof(IViewSourceLoader), services.ViewSourceLoader);

            viewComponentFactory = new DefaultViewComponentFactory();
            viewComponentFactory.Initialize();
            services.AddService(typeof(IViewComponentFactory), viewComponentFactory);
            services.AddService(typeof(IViewComponentRegistry), viewComponentFactory.Registry);

            viewComponentFactory.Registry.AddViewComponent("Widget", typeof(WidgetComponent));

            factory = new SparkViewFactory();
            factory.Service(services);

            controller = mocks.CreateMock<IController>();
            controllerContext = new ControllerContext();
            engineContext = new MockEngineContext(new UrlInfo("", "Home", "Index", "/", "castle"));
            engineContext.AddService(typeof(IViewComponentFactory), viewComponentFactory);
            engineContext.AddService(typeof(IViewComponentRegistry), viewComponentFactory.Registry);
        }

        [Test]
        public void ComponentCallingRenderView()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process("Home\\ComponentCallingRenderView.spark", writer, engineContext, controller, controllerContext);

            var output = writer.ToString();
            Assert.IsTrue(output.Contains("This is a widget"));
        }

        [Test]
        public void ComponentRenderViewWithParameters()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process("Home\\ComponentRenderViewWithParameters.spark", writer, engineContext, controller, controllerContext);

            var output = writer.ToString();
            Assert.IsTrue(output.Contains("Mode Alpha and 123"));
            Assert.IsTrue(output.Contains("Mode Beta and 456"));
        }

        [Test]
        public void ComponentRenderViewWithContent()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process("Home\\ComponentRenderViewWithContent.spark", writer, engineContext, controller, controllerContext);

            var output = writer.ToString();
            Assert.IsTrue(output.Contains("Mode Delta and 789"));
            Assert.IsTrue(output.Contains("<p class=\"message\">!!Delta!!</p>"));
        }

        [ViewComponentDetails("WidgetComponent")]
        public class WidgetComponent : ViewComponent
        {
            [ViewComponentParam]
            public string Mode { get; set; }

            [ViewComponentParam]
            public string ExtraData { get; set; }

            public override void Render()
            {
                if (string.IsNullOrEmpty(Mode))
                {
                    RenderView("default");
                    return;
                }

                PropertyBag["Mode"] = Mode;
                PropertyBag["ExtraData"] = ExtraData;
                RenderView("withextradata");
            }
        }
    }
}
