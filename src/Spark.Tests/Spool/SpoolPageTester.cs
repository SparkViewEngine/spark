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
            Assert.Multiple(() =>
            {
                Assert.That(page.Next, Is.Null);
                Assert.That(page.Count, Is.EqualTo(1));
            });
            Assert.That(page.Buffer[0], Is.EqualTo("hello"));
        }

        [Test]
        public void AppendMultiple()
        {
            var page = new SpoolPage();
            page.Append("hello");
            page.Append("world");
            Assert.Multiple(() =>
            {
                Assert.That(page.Next, Is.Null);
                Assert.That(page.Count, Is.EqualTo(2));
            });
            Assert.Multiple(() =>
            {
                Assert.That(page.Buffer[0], Is.EqualTo("hello"));
                Assert.That(page.Buffer[1], Is.EqualTo("world"));
            });
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

            Assert.Multiple(() =>
            {
                Assert.That(last, Is.Not.SameAs(page));
                Assert.That(page.Count, Is.EqualTo(SpoolPage.BUFFER_SIZE));
            });
            Assert.That(last.Count, Is.EqualTo(30));
            Assert.That(last.Buffer[0], Is.EqualTo(SpoolPage.BUFFER_SIZE.ToString()));
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

            Assert.That(second.Count, Is.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(second.Buffer[0], Is.EqualTo("1"));
                Assert.That(second.Buffer[1], Is.EqualTo("2"));
                Assert.That(second.Buffer[2], Is.EqualTo("3"));
            });
        }

    }
}
