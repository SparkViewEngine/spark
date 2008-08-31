using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework.ViewComponents;
using NUnit.Framework;

namespace Castle.MonoRail.Views.Spark.Tests.ViewComponents
{
    [TestFixture]
    public class AllFrameworkComponentTests : BaseViewComponentTests
    {
        [Test]
        public void AuthenticatedContent()
        {
            var content = ExecuteView("Home\\AllFrameworkComponents-AuthenticatedContent.spark");
            Assert.That(content.Contains("two"));
            Assert.IsFalse(content.Contains("one"));
        }
        [Test]
        public void CaptureFor()
        {
            var content = ExecuteView("Home\\AllFrameworkComponents-CaptureFor.spark");
            Assert.That(content.Contains("onetwothreefour"));
        }
        [Test]
        public void ChildContent()
        {
            var content = ExecuteView("Home\\AllFrameworkComponents-ChildContent.spark");
            ContainsInOrder(content, "one", "5hello", "two");
        }
        [Test]
        public void ColumnRenderer()
        {
            var content = ExecuteView("Home\\AllFrameworkComponents-ColumnRenderer.spark");
            ContainsInOrder(content, 
                "*start*",
                "*firstelement*",
                "*a*",
                "*b*",
                "*c*",
                "*d*",
                "*e*",  
                "*f*",
                "*g*",
                "*h*",
                "*endblock*");
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void DiggStylePagination()
        {
            var content = ExecuteView("Home\\AllFrameworkComponents-DiggStylePagination.spark");
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void Security()
        {
            var content = ExecuteView("Home\\AllFrameworkComponents-Security.spark");
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void SelectStylePagination()
        {
            var content = ExecuteView("Home\\AllFrameworkComponents-SelectStylePagination.spark");
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void SiteMap()
        {
            var content = ExecuteView("Home\\AllFrameworkComponents-SiteMap.spark");
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void TreeMaker()
        {
            var content = ExecuteView("Home\\AllFrameworkComponents-TreeMaker.spark");
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void UpdatePage()
        {
            var content = ExecuteView("Home\\AllFrameworkComponents-UpdatePage.spark");
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void UpdateTag()
        {
            var content = ExecuteView("Home\\AllFrameworkComponents-UpdateTag.spark");
        }

        string ExecuteView(string page)
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(page, writer, engineContext, controller, controllerContext);

            mocks.VerifyAll();

            Assert.IsNotNull(writer.ToString());
            Assert.IsNotEmpty(writer.ToString());
            return writer.ToString();
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
    }
}
