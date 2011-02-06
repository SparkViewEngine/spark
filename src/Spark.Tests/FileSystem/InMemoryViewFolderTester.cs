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
using NUnit.Framework.SyntaxHelpers;

namespace Spark.Tests.FileSystem
{
    [TestFixture]
    public class InMemoryViewFolderTester 
    {
        [Test]
        public void HasViewCaseInsensitive()
        {
            var folder = new InMemoryViewFolder();
            Assert.IsFalse(folder.HasView(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar)));
            folder.Add(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar), "stuff");
            Assert.IsTrue(folder.HasView(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar)));
            Assert.IsFalse(folder.HasView(string.Format("Home{0}Index", Path.DirectorySeparatorChar)));
            Assert.IsTrue(folder.HasView(string.Format("Home{0}index.spark", Path.DirectorySeparatorChar)));
            Assert.IsTrue(folder.HasView(string.Format("home{0}INDEX.SPARK", Path.DirectorySeparatorChar)));
        }

        [Test]
        public void ListViewsInFolder()
        {
            var folder = new InMemoryViewFolder
                             {
                                 {string.Format("Home{0}Alpha.spark", Path.DirectorySeparatorChar), "stuff"},
                                 {string.Format("Home{0}Beta.spark", Path.DirectorySeparatorChar), "stuff"},
                                 {string.Format("Home2{0}Gamma.spark", Path.DirectorySeparatorChar), "stuff"},
                                 {string.Format("home{0}Delta.spark", Path.DirectorySeparatorChar), "stuff"},
                                 {string.Format("Home{0}Something{0}else.spark", Path.DirectorySeparatorChar), "stuff"}
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
            folder.Add(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar), "stuff");
            folder.GetViewSource(string.Format("Home{0}List.spark", Path.DirectorySeparatorChar));            
        }

        [Test]
        public void ReadFileContents()
        {
            var folder = new InMemoryViewFolder();
            folder.Add(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar), "this is the file contents");
            var source = folder.GetViewSource(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar));
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
            folder.Add(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar), "this is the file contents");
            var source1 = folder.GetViewSource(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar));
            var lastModified1 = source1.LastModified;

            folder.Set(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar), "this is the file contents");
            var source2 = folder.GetViewSource(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar));
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
            folder.Add(string.Format("home{0}index.spark", Path.DirectorySeparatorChar), "<p>Hello world</p>");
            var engine = new SparkViewEngine(new SparkSettings().SetPageBaseType(typeof (StubSparkView))){ViewFolder = folder};

            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add(string.Format("home{0}index.spark", Path.DirectorySeparatorChar));
            var view = engine.CreateInstance(descriptor);
            var contents = view.RenderView();
            Assert.AreEqual("<p>Hello world</p>", contents);
        }

        static string ReadToEnd(IViewFolder viewFolder, string path)
        {
            using (var stream = viewFolder.GetViewSource(path).OpenViewStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        static string RenderView(ISparkViewEngine engine, string path)
        {
            var descriptor = new SparkViewDescriptor()
                .AddTemplate(path);

            return engine
                .CreateInstance(descriptor)                
                .RenderView();
        }

        [Test]
        public void UnicodeCharactersSurviveConversionToByteArrayAndBack()
        {
            var folder = new InMemoryViewFolder();
            folder.Add(string.Format("Home{0}fr.spark", Path.DirectorySeparatorChar), "Fran\u00E7ais");
            folder.Add(string.Format("Home{0}ru.spark", Path.DirectorySeparatorChar), "\u0420\u0443\u0441\u0441\u043A\u0438\u0439");
            folder.Add(string.Format("Home{0}ja.spark", Path.DirectorySeparatorChar), "\u65E5\u672C\u8A9E");

            Assert.That(ReadToEnd(folder, string.Format("Home{0}fr.spark", Path.DirectorySeparatorChar)), Is.EqualTo("Français"));
            Assert.That(ReadToEnd(folder, string.Format("Home{0}ru.spark", Path.DirectorySeparatorChar)), Is.EqualTo("Русский"));
            Assert.That(ReadToEnd(folder, string.Format("Home{0}ja.spark", Path.DirectorySeparatorChar)), Is.EqualTo("日本語"));
            
            var settings = new SparkSettings().SetPageBaseType(typeof(StubSparkView));
            var engine = new SparkViewEngine(settings) { ViewFolder = folder };
            Assert.That(RenderView(engine, string.Format("Home{0}fr.spark", Path.DirectorySeparatorChar)), Is.EqualTo("Français"));
            Assert.That(RenderView(engine, string.Format("Home{0}ru.spark", Path.DirectorySeparatorChar)), Is.EqualTo("Русский"));
            Assert.That(RenderView(engine, string.Format("Home{0}ja.spark", Path.DirectorySeparatorChar)), Is.EqualTo("日本語"));
        }
    }
}
