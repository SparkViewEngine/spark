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
using System.Linq;
using NUnit.Framework;

namespace Spark.FileSystem
{
    [TestFixture]
    public class CombinedViewFolderTester
    {
        [Test]
        public void HasTemplate()
        {
            var first = new InMemoryViewFolder { { "one.txt", "one" } };
            var second = new InMemoryViewFolder { { "two.txt", "two" } };
            var viewFolder = new CombinedViewFolder(first, second);

            Assert.Multiple(() =>
            {
                Assert.That(viewFolder.HasView("one.txt"), Is.True);
                Assert.That(viewFolder.HasView("two.txt"), Is.True);
                Assert.That(viewFolder.HasView("three.txt"), Is.False);
            });
        }

        [Test]
        public void OpenFileStream()
        {
            var first = new InMemoryViewFolder { { "one.txt", "one" } };
            var second = new InMemoryViewFolder { { "two.txt", "two" } };
            var viewFolder = new CombinedViewFolder(first, second);

            using (var reader = new StreamReader(viewFolder.GetViewSource("one.txt").OpenViewStream()))
            {
                var content = reader.ReadToEnd();
                Assert.That(content, Is.EqualTo("one"));
            }

            using (var reader = new StreamReader(viewFolder.GetViewSource("two.txt").OpenViewStream()))
            {
                var content = reader.ReadToEnd();
                Assert.That(content, Is.EqualTo("two"));
            }
        }

        [Test]
        public void OpenMissingFile()
        {
            var first = new InMemoryViewFolder { { "one.txt", "one" } };
            var second = new InMemoryViewFolder { { "two.txt", "two" } };
            var viewFolder = new CombinedViewFolder(first, second);

            Assert.That(() => viewFolder.GetViewSource("three.txt"), Throws.TypeOf<FileNotFoundException>());
        }


        [Test]
        public void OpenFromFirstViewFolder()
        {
            var first = new InMemoryViewFolder { { "one.txt", "one" } };
            var second = new InMemoryViewFolder { { "one.txt", "two" } };
            var viewFolder = new CombinedViewFolder(first, second);

            using (var reader = new StreamReader(viewFolder.GetViewSource("one.txt").OpenViewStream()))
            {
                var content = reader.ReadToEnd();
                Assert.That(content, Is.EqualTo("one"));
            }
        }

        [Test]
        public void ListFilesWithDedupe()
        {
            var first = new InMemoryViewFolder
            {
                { Path.Combine("home", "three.txt"), "three" },
                { Path.Combine("home", "one.txt"), "one" }
            };
            var second = new InMemoryViewFolder
            {
                { Path.Combine("home", "two.txt"), "two" },
                { Path.Combine("home", "three.txt"), "three" }
            };

            var viewFolder = new CombinedViewFolder(first, second);
            var views = viewFolder.ListViews("home");

            Assert.That(views.Count, Is.EqualTo(3));
            Assert.That(views.ToArray(), Does.Contain(Path.Combine("home", "one.txt")));
            Assert.That(views.ToArray(), Does.Contain(Path.Combine("home", "two.txt")));
            Assert.That(views.ToArray(), Does.Contain(Path.Combine("home", "three.txt")));
        }
    }
}
