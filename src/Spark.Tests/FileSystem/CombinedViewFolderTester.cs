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
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests.FileSystem
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

            Assert.IsTrue(viewFolder.HasView("one.txt"));
            Assert.IsTrue(viewFolder.HasView("two.txt"));
            Assert.IsFalse(viewFolder.HasView("three.txt"));
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
                Assert.AreEqual("one", content);
            }

            using (var reader = new StreamReader(viewFolder.GetViewSource("two.txt").OpenViewStream()))
            {
                var content = reader.ReadToEnd();
                Assert.AreEqual("two", content);
            }
        }

        [Test, ExpectedException(typeof(FileNotFoundException))]
        public void OpenMissingFile()
        {
            var first = new InMemoryViewFolder { { "one.txt", "one" } };
            var second = new InMemoryViewFolder { { "two.txt", "two" } };
            var viewFolder = new CombinedViewFolder(first, second);

            viewFolder.GetViewSource("three.txt");
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
                Assert.AreEqual("one", content);
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
			
            Assert.AreEqual(3, views.Count);
            Assert.Contains(Path.Combine("home", "one.txt"), views.ToArray());
            Assert.Contains(Path.Combine("home", "two.txt"), views.ToArray());
            Assert.Contains(Path.Combine("home", "three.txt"), views.ToArray());
        }		
    }
}
