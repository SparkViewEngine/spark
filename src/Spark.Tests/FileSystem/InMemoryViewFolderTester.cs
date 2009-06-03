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
using Spark.FileSystem;
using Spark.Tests.Stubs;

namespace Spark.Tests
{
    [TestFixture]
    public class InMemoryViewFolderTester 
    {
        [Test]
        public void HasViewCaseInsensitive()
        {
            var folder = new InMemoryViewFolder();
            Assert.IsFalse(folder.HasView("Home\\Index.spark".AsPath()));
            folder.Add("Home\\Index.spark".AsPath(), "stuff");
            Assert.IsTrue(folder.HasView("Home\\Index.spark".AsPath()));
            Assert.IsFalse(folder.HasView("Home\\Index".AsPath()));
            Assert.IsTrue(folder.HasView("Home\\index.spark".AsPath()));
            Assert.IsTrue(folder.HasView("home\\INDEX.SPARK".AsPath()));
        }

        [Test]
        public void ListViewsInFolder()
        {
            var folder = new InMemoryViewFolder
                             {
                                 {"Home\\Alpha.spark".AsPath(), "stuff"},
                                 {"Home\\Beta.spark".AsPath(), "stuff"},
                                 {"Home2\\Gamma.spark".AsPath(), "stuff"},
                                 {"home\\Delta.spark".AsPath(), "stuff"},
                                 {"Home\\Something\\else.spark".AsPath(), "stuff"}
                             };

            var views = folder.ListViews("Home");

            var baseNames = views.Select(v => Path.GetFileNameWithoutExtension(v)).ToArray();
            Assert.AreEqual(3, baseNames.Count());
            Assert.Contains("Alpha", baseNames);
            Assert.Contains("Beta", baseNames);
            Assert.Contains("Delta", baseNames);
        }

        [Test, ExpectedException(typeof(FileNotFoundException))]
        public void FileNotFoundException()
        {
            var folder = new InMemoryViewFolder();
            folder.Add("Home\\Index.spark".AsPath(), "stuff");
            folder.GetViewSource("Home\\List.spark".AsPath());            
        }

        [Test]
        public void ReadFileContents()
        {
            var folder = new InMemoryViewFolder();
            folder.Add("Home\\Index.spark".AsPath(), "this is the file contents");
            var source = folder.GetViewSource("Home\\Index.spark".AsPath());
            using (var stream = source.OpenViewStream())
            {
                using(var reader = new StreamReader(stream))
                {
                    var contents = reader.ReadToEnd();
                    Assert.AreEqual("this is the file contents", contents);
                }
            }
        }

        [Test]
        public void LastModifiedChanges()
        {
            var folder = new InMemoryViewFolder();
            folder.Add("Home\\Index.spark".AsPath(), "this is the file contents");
            var source1 = folder.GetViewSource("Home\\Index.spark".AsPath());
            var lastModified1 = source1.LastModified;

            folder.Set("Home\\Index.spark".AsPath(), "this is the file contents");
            var source2 = folder.GetViewSource("Home\\Index.spark".AsPath());
            var lastModified2 = source2.LastModified;

            Assert.AreNotEqual(lastModified1, lastModified2);

            var lastModified1b = source1.LastModified;
            var lastModified2b = source1.LastModified;

            Assert.AreNotEqual(lastModified1, lastModified1b);
            Assert.AreEqual(lastModified1b, lastModified2b);

        }

        [Test]
        public void InMemoryViewFolderUsedByEngine()
        {
            var folder = new InMemoryViewFolder();
            folder.Add("home\\index.spark".AsPath(), "<p>Hello world</p>");
            var engine = new SparkViewEngine(new SparkSettings().SetPageBaseType(typeof (StubSparkView))){ViewFolder = folder};

            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add("home\\index.spark".AsPath());
            var view = engine.CreateInstance(descriptor);
            var contents = view.RenderView();
            Assert.AreEqual("<p>Hello world</p>", contents);
        }
    }
}
