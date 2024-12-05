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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using NUnit.Framework;

namespace Castle.MonoRail.Views.Spark.Tests.ViewComponents
{
    [TestFixture]
    public class ViewComponentRenderBodyTests : BaseViewComponentTests
    {
        public override void Init()
        {
            base.Init();

            viewComponentFactory.Registry.AddViewComponent("ComponentWithBody", typeof(ComponentWithBody));
            viewComponentFactory.Registry.AddViewComponent("ComponentWithBodyAndNoDetailsAttrib", typeof(ComponentWithBodyAndNoDetailsAttrib));
        }

        [Test]
        public void ComponentBodySimpleHtml()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(string.Format("Home{0}ComponentBodySimpleHtml.spark", Path.DirectorySeparatorChar), writer,
                            engineContext, controller, controllerContext);

            mocks.VerifyAll();

            var content = writer.ToString();
            Assert.That(content, Does.Contain("<p>This is text</p>"));
            Assert.Multiple(() =>
            {
                Assert.That(content.Contains("<ComponentWithBody>"), Is.False);
                Assert.That(content.Contains("</ComponentWithBody>"), Is.False);
            });
        }


        [Test]
        public void ComponentBodyHtmlHasAttributes()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(string.Format("Home{0}ComponentBodyHtmlHasAttributes.spark", Path.DirectorySeparatorChar),
                            writer, engineContext, controller, controllerContext);

            mocks.VerifyAll();

            var content = writer.ToString();
            Assert.That(content, Does.Contain("<script src=\"foo.js\"></script>"));
            Assert.That(content, Does.Contain("<link href=\"bar.css\"/>"));
            Assert.Multiple(() =>
            {
                Assert.That(content.Contains("<ComponentWithBody>"), Is.False);
                Assert.That(content.Contains("</ComponentWithBody>"), Is.False);
            });
        }


        [Test]
        public void RenderingComponentWithBodyAndNoDetailsAttrib()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(
                string.Format("Home{0}RenderingComponentWithBodyAndNoDetailsAttrib.spark", Path.DirectorySeparatorChar),
                writer, engineContext, controller, controllerContext);

            mocks.VerifyAll();

            var content = writer.ToString();
            Assert.That(content, Does.Contain("<p>This is text</p>"));
            Assert.Multiple(() =>
            {
                Assert.That(content.Contains("<ComponentWithBodyAndNoDetailsAttrib>"), Is.False);
                Assert.That(content.Contains("</ComponentWithBodyAndNoDetailsAttrib>"), Is.False);
            });
        }
    }

    [ViewComponentDetails("ComponentWithBody")]
    public class ComponentWithBody : ViewComponent
    {
        public override void Render()
        {
            RenderBody();
        }
    }

    public class ComponentWithBodyAndNoDetailsAttrib : ViewComponent
    {
        public override void Render()
        {
            RenderBody();
        }
    }

}
