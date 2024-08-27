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
using NUnit.Framework;
using Spark.Spool;

namespace Spark.Tests.Spool
{
    [TestFixture]
    public class SpoolPageTester
    {
        [Test]
        public void Appending()
        {
            var page = new SpoolPage();
            page.Append("hello");
            Assert.IsNull(page.Next);
            Assert.AreEqual(1, page.Count);
            Assert.AreEqual("hello", page.Buffer[0]);
        }

        [Test]
        public void AppendMultiple()
        {
            var page = new SpoolPage();
            page.Append("hello");
            page.Append("world");
            Assert.IsNull(page.Next);
            Assert.AreEqual(2, page.Count);
            Assert.AreEqual("hello", page.Buffer[0]);
            Assert.AreEqual("world", page.Buffer[1]);
        }

        [Test]
        public void AppendOverBoundary()
        {
            var page = new SpoolPage();
            var last = page;
            for (int index = 0; index != SpoolPage.BUFFER_SIZE + 30; ++index)
            {
                last = last.Append(index.ToString());
            }
            Assert.AreNotSame(page, last);
            Assert.AreEqual(SpoolPage.BUFFER_SIZE, page.Count);
            Assert.AreEqual(30, last.Count);
            Assert.AreEqual(SpoolPage.BUFFER_SIZE.ToString(), last.Buffer[0]);
        }

        [Test]
        public void CallsReleaseOnNextOnlyIfIsNotReleasedAlready()
        {
            var first = new SpoolPage();
            var next = first;
            for (int index = 0; index != SpoolPage.BUFFER_SIZE + 30; ++index)
            {
                next = next.Append(index.ToString());
            }
            next.Release();

            var second = new SpoolPage();
            second.Append("1");
            second.Append("2");
            second.Append("3");

            first.Release();

            Assert.AreEqual(3, second.Count);
            Assert.AreEqual("1", second.Buffer[0]);
            Assert.AreEqual("2", second.Buffer[1]);
            Assert.AreEqual("3", second.Buffer[2]);
        }

    }
}
