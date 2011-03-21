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
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-AuthenticatedContent.spark", Path.DirectorySeparatorChar));
            Assert.That(content.Contains("two"));
            Assert.IsFalse(content.Contains("one"));
        }
        [Test]
        public void CaptureFor()
        {
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-CaptureFor.spark", Path.DirectorySeparatorChar));
            Assert.That(content.Contains("onetwothreefour"));
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
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-DiggStylePagination.spark", Path.DirectorySeparatorChar));
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void Security()
        {
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-Security.spark", Path.DirectorySeparatorChar));
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void SelectStylePagination()
        {
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-SelectStylePagination.spark", Path.DirectorySeparatorChar));
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void SiteMap()
        {
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-SiteMap.spark", Path.DirectorySeparatorChar));
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void TreeMaker()
        {
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-TreeMaker.spark", Path.DirectorySeparatorChar));
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void UpdatePage()
        {
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-UpdatePage.spark", Path.DirectorySeparatorChar));
        }
        [Test, Ignore("Creating a test for each built-in component")]
        public void UpdateTag()
        {
            var content = ExecuteView(string.Format("Home{0}AllFrameworkComponents-UpdateTag.spark", Path.DirectorySeparatorChar));
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
