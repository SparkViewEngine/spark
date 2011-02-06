//-------------------------------------------------------------------------
// <copyright file="Constraints.cs">
// Copyright 2008-2010 Louis DeJardin - http://whereslou.com
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
// </copyright>
// <author>Louis DeJardin</author>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

namespace Castle.MonoRail.Views.Spark.Tests.ViewComponents
{
    using System;
    using System.IO;
    using Castle.MonoRail.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class ViewComponentSectionTests : BaseViewComponentTests
    {
        public override void Init()
        {
            base.Init();
            viewComponentFactory.Registry.AddViewComponent("ComponentWithSections", typeof(ComponentWithSections));
        }

        [Test]
        public void ComponentWithSimpleSections()
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(string.Format("Home{0}ComponentWithSimpleSections.spark", Path.DirectorySeparatorChar),
                            writer, engineContext, controller, controllerContext);

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
            factory.Process(string.Format("Home{0}ComponentWithComplexSections.spark", Path.DirectorySeparatorChar),
                            writer, engineContext, controller, controllerContext);

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
            factory.Process(string.Format("Home{0}ComponentWithComplexSections.spark", Path.DirectorySeparatorChar),
                            writer, engineContext, controller, controllerContext);

            var output = writer.ToString();
            Assert.That(output, Contains.InOrder(
                "1,2,3,",
                "<span>10</span>",
                "<span>9</span>",
                "<span>8</span>"));
            Assert.IsFalse(output.Contains("for each"));
            Assert.IsFalse(output.Contains("span each"));
        }

		[Test]
		public void ComponentWithPartialsInSection()
		{
			mocks.ReplayAll();

			var writer = new StringWriter();
		    factory.Process(string.Format("Home{0}ComponentWithPartialsInSection.spark", Path.DirectorySeparatorChar), writer,
		                    engineContext, controller, controllerContext);

			var output = writer.ToString();
			Assert.IsTrue(output.Contains("this is some text: test123"));
		}

		[Test]
		public void NestedComponentInSection()
		{
			mocks.ReplayAll();

			var writer = new StringWriter();
		    factory.Process(string.Format("Home{0}NestedComponentInSection.spark", Path.DirectorySeparatorChar), writer,
		                    engineContext, controller, controllerContext);

			var output = writer.ToString();
			Assert.IsTrue(output.Contains("header1"));
			Assert.IsTrue(output.Contains("header2"));
			Assert.IsTrue(output.Contains("body1"));
			Assert.IsTrue(output.Contains("body2"));
			Assert.IsTrue(output.Contains("footer1"));
			Assert.IsTrue(output.Contains("footer2"));

			Assert.IsFalse(output.Contains("<header>"));
			Assert.IsFalse(output.Contains("<body>"));
			Assert.IsFalse(output.Contains("<footer>"));
			Assert.IsFalse(output.Contains("</ComponentWithSections>"));
		}

        [ViewComponentDetails("ComponentWithSections",Sections="header,body,footer")]
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
}
