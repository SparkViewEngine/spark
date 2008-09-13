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
            factory.Process("Home\\ComponentBodySimpleHtml.spark", writer, engineContext, controller, controllerContext);

            mocks.VerifyAll();

            var content = writer.ToString();
            Assert.That(content.Contains("<p>This is text</p>"));
            Assert.IsFalse(content.Contains("<ComponentWithBody>"));
            Assert.IsFalse(content.Contains("</ComponentWithBody>"));
        }


        [Test]
        public void ComponentBodyHtmlHasAttributes()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process("Home\\ComponentBodyHtmlHasAttributes.spark", writer, engineContext, controller, controllerContext);

            mocks.VerifyAll();

            var content = writer.ToString();
            Assert.That(content.Contains("<script src=\"foo.js\"></script>"));
            Assert.That(content.Contains("<link href=\"bar.css\"/>"));
            Assert.IsFalse(content.Contains("<ComponentWithBody>"));
            Assert.IsFalse(content.Contains("</ComponentWithBody>"));
        }

        
        [Test]
        public void RenderingComponentWithBodyAndNoDetailsAttrib()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process("Home\\RenderingComponentWithBodyAndNoDetailsAttrib.spark", writer, engineContext, controller, controllerContext);

            mocks.VerifyAll();

            var content = writer.ToString();
            Assert.That(content.Contains("<p>This is text</p>"));
            Assert.IsFalse(content.Contains("<ComponentWithBodyAndNoDetailsAttrib>"));
            Assert.IsFalse(content.Contains("</ComponentWithBodyAndNoDetailsAttrib>"));
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
