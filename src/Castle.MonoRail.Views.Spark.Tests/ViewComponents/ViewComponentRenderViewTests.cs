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
using System.IO;
using System.Reflection;
using Castle.MonoRail.Framework;
using NUnit.Framework;
using Spark.FileSystem;

namespace Castle.MonoRail.Views.Spark.Tests.ViewComponents
{
    [TestFixture]
    public class ViewComponentRenderViewTests : BaseViewComponentTests
    {
        public override void Init()
        {
            base.Init();

            viewComponentFactory.Registry.AddViewComponent("Widget", typeof(WidgetComponent));
        }


        [Test]
        public void ComponentCallingRenderView()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(string.Format("Home{0}ComponentCallingRenderView.spark", Path.DirectorySeparatorChar),
                            writer, engineContext, controller, controllerContext);

            var output = writer.ToString();
            Assert.IsTrue(output.Contains("This is a widget"));
        }

        [Test]
        public void ComponentRenderViewWithParameters()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(
                string.Format("Home{0}ComponentRenderViewWithParameters.spark", Path.DirectorySeparatorChar), writer,
                engineContext, controller, controllerContext);

            var output = writer.ToString();
            Assert.IsTrue(output.Contains("Mode Alpha and 123"));
            Assert.IsTrue(output.Contains("Mode Beta and 456"));
        }

        [Test]
        public void ComponentRenderViewWithContent()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(string.Format("Home{0}ComponentRenderViewWithContent.spark", Path.DirectorySeparatorChar),
                            writer, engineContext, controller, controllerContext);

            var output = writer.ToString();
            Assert.IsTrue(output.Contains("Mode Delta and 789"));
            Assert.IsTrue(output.Contains("<p class=\"message\">!!Delta!!</p>"));
        }

        [Test]
        public void ComponentRenderViewFromEmbeddedResource()
        {
            viewComponentFactory.Registry.AddViewComponent("UseEmbeddedViews", typeof(UseEmbeddedViews));

            var embeddedViewFolder = new EmbeddedViewFolder(
                Assembly.Load("Castle.MonoRail.Views.Spark.Tests"),
                "Castle.MonoRail.Views.Spark.Tests.EmbeddedViews");

            engine.ViewFolder = engine.ViewFolder.Append(embeddedViewFolder);

            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(
                string.Format("Home{0}ComponentRenderViewFromEmbeddedResource.spark", Path.DirectorySeparatorChar),
                writer, engineContext, controller, controllerContext);

            mocks.VerifyAll();

            var content = writer.ToString();
            Assert.That(content.Contains("<p>This was embedded</p>"));
        }

        [Test]
        public void ComponentRenderViewSharesOnceFlags()
        {
            viewComponentFactory.Registry.AddViewComponent("OnceWidget", typeof(OnceWidget));

            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(
                string.Format("Home{0}ComponentRenderViewSharesOnceFlags.spark", Path.DirectorySeparatorChar), writer,
                engineContext, controller, controllerContext);

            mocks.VerifyAll();

            var content = writer.ToString();
            Assert.That(content.Contains("<p>ok1</p>"));
            Assert.That(content.Contains("<p>ok2</p>"));
            Assert.IsFalse(content.Contains("fail"));
        }

        [Test]
        public void ComponentRenderViewUsesGlobalSpark()
        {
            viewComponentFactory.Registry.AddViewComponent("UsesGlobalSpark", typeof(UsesGlobalSpark));

            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(
                string.Format("Home{0}ComponentRenderViewUsesGlobalSpark.spark", Path.DirectorySeparatorChar), writer,
                engineContext, controller, controllerContext);

            mocks.VerifyAll();

            var content = writer.ToString();
            Assert.That(content.Contains("<p>ok1</p>"));
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

        [ViewComponentDetails("UseEmbeddedViews")]
        public class UseEmbeddedViews : ViewComponent
        {
            public override void Render()
            {
                RenderView("default");
            }
        }


        [ViewComponentDetails("OnceWidget")]
        public class OnceWidget : ViewComponent
        {
            public override void Render()
            {
                RenderView("default");
            }
        }


        [ViewComponentDetails("UsesGlobalSpark")]
        public class UsesGlobalSpark : ViewComponent
        {
            public override void Render()
            {
                RenderView("default");
            }
        }
    }
}