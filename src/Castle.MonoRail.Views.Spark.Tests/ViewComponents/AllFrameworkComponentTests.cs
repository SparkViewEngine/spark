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
using NUnit.Framework;

namespace Castle.MonoRail.Views.Spark.Tests.ViewComponents
{
    [TestFixture]
    public class AllFrameworkComponentTests : BaseViewComponentTests
    {
        [Test]
        public void AuthenticatedContent()
        {
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-AuthenticatedContent.spark", Path.DirectorySeparatorChar));

            Assert.That(content, Does.Contain("two"));
            Assert.That(content.Contains("one"), Is.False);
        }

        [Test]
        public void CaptureFor()
        {
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-CaptureFor.spark", Path.DirectorySeparatorChar));

            Assert.That(content, Does.Contain("onetwothreefour"));
        }

        [Test]
        public void ChildContent()
        {
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-ChildContent.spark", Path.DirectorySeparatorChar));

            ContainsInOrder(content, "one", "5hello", "two");
        }

        [Test]
        public void ColumnRenderer()
        {
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-ColumnRenderer.spark", Path.DirectorySeparatorChar));

            ContainsInOrder(
                content,
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
            _ = ExecuteView(string.Format("Home{0}AllFrameworkComponents-DiggStylePagination.spark", Path.DirectorySeparatorChar));
        }

        [Test, Ignore("Creating a test for each built-in component")]
        public void Security()
        {
            _ = ExecuteView(string.Format("Home{0}AllFrameworkComponents-Security.spark", Path.DirectorySeparatorChar));
        }

        [Test, Ignore("Creating a test for each built-in component")]
        public void SelectStylePagination()
        {
            _ = ExecuteView(string.Format("Home{0}AllFrameworkComponents-SelectStylePagination.spark", Path.DirectorySeparatorChar));
        }

        [Test, Ignore("Creating a test for each built-in component")]
        public void SiteMap()
        {
            _ = ExecuteView(string.Format("Home{0}AllFrameworkComponents-SiteMap.spark", Path.DirectorySeparatorChar));
        }

        [Test, Ignore("Creating a test for each built-in component")]
        public void TreeMaker()
        {
            _ = ExecuteView(string.Format("Home{0}AllFrameworkComponents-TreeMaker.spark", Path.DirectorySeparatorChar));
        }

        [Test, Ignore("Creating a test for each built-in component")]
        public void UpdatePage()
        {
            _ = ExecuteView(string.Format("Home{0}AllFrameworkComponents-UpdatePage.spark", Path.DirectorySeparatorChar));
        }

        [Test, Ignore("Creating a test for each built-in component")]
        public void UpdateTag()
        {
            _ = ExecuteView(string.Format("Home{0}AllFrameworkComponents-UpdateTag.spark", Path.DirectorySeparatorChar));
        }

        string ExecuteView(string page)
        {
            mocks.ReplayAll();

            var writer = new StringWriter();
            factory.Process(page, writer, engineContext, controller, controllerContext);

            mocks.VerifyAll();

            Assert.That(writer.ToString(), Is.Not.Null);
            Assert.That(writer.ToString(), Is.Not.Empty);
            return writer.ToString();
        }

        static void ContainsInOrder(string content, params string[] values)
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
