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
            Assert.IsFalse(folder.HasView(Path.Combine("Home", "Index.spark")));
            folder.Add(Path.Combine("Home", "Index.spark"), "stuff");
            Assert.IsTrue(folder.HasView(Path.Combine("Home", "Index.spark")));
            Assert.IsFalse(folder.HasView(Path.Combine("Home", "Index")));
            Assert.IsTrue(folder.HasView(Path.Combine("Home", "index.spark")));
            Assert.IsTrue(folder.HasView(Path.Combine("home", "INDEX.SPARK")));
        }

        [Test]
        public void ListViewsInFolder()
        {
            var folder = new InMemoryViewFolder
                             {
                                 {Path.Combine("Home", "Alpha.spark"), "stuff"},
                                 {Path.Combine("Home", "Beta.spark"), "stuff"},
                                 {Path.Combine("Home2", "Gamma.spark"), "stuff"},
                                 {Path.Combine("home", "Delta.spark"), "stuff"},
                                 {Path.Combine("Home","Something","else.spark"), "stuff"}
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
            folder.Add(Path.Combine("Home", "Index.spark"), "stuff");
            folder.GetViewSource(Path.Combine("Home", "List.spark"));            
        }

        [Test]
        public void ReadFileContents()
        {
            var folder = new InMemoryViewFolder();
            folder.Add(Path.Combine("Home", "Index.spark"), "this is the file contents");
            var source = folder.GetViewSource(Path.Combine("Home", "Index.spark"));
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
            folder.Add(Path.Combine("Home", "Index.spark"), "this is the file contents");
            var source1 = folder.GetViewSource(Path.Combine("Home", "Index.spark"));
            var lastModified1 = source1.LastModified;
            
            folder.Set(Path.Combine("Home", "Index.spark"), "this is the file contents");
            var source2 = folder.GetViewSource(Path.Combine("Home", "Index.spark"));
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
            folder.Add(Path.Combine("home", "index.spark"), "<p>Hello world</p>");
            var engine = new SparkViewEngine(new SparkSettings().SetPageBaseType(typeof (StubSparkView))){ViewFolder = folder};

            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add(Path.Combine("home", "index.spark"));
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
            folder.Add(Path.Combine("Home", "fr.spark"), "Fran\u00E7ais");
            folder.Add(Path.Combine("Home", "ru.spark"), "\u0420\u0443\u0441\u0441\u043A\u0438\u0439");
            folder.Add(Path.Combine("Home", "ja.spark"), "\u65E5\u672C\u8A9E");

            Assert.That(ReadToEnd(folder, Path.Combine("Home", "fr.spark")), Is.EqualTo("Français"));
            Assert.That(ReadToEnd(folder, Path.Combine("Home", "ru.spark")), Is.EqualTo("Русский"));
            Assert.That(ReadToEnd(folder, Path.Combine("Home", "ja.spark")), Is.EqualTo("日本語"));
            
            var settings = new SparkSettings().SetPageBaseType(typeof(StubSparkView));
            var engine = new SparkViewEngine(settings) { ViewFolder = folder };
            Assert.That(RenderView(engine, Path.Combine("Home", "fr.spark")), Is.EqualTo("Français"));
            Assert.That(RenderView(engine, Path.Combine("Home", "ru.spark")), Is.EqualTo("Русский"));
            Assert.That(RenderView(engine, Path.Combine("Home", "ja.spark")), Is.EqualTo("日本語"));
        }
    }
}
