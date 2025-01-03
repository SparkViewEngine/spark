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

using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Spark.Extensions;
using Spark.Tests;
using Spark.Tests.Stubs;

namespace Spark.FileSystem
{
    [TestFixture]
    public class InMemoryViewFolderTester
    {
        [Test]
        public void HasViewCaseInsensitive()
        {
            var viewFolder = new InMemoryViewFolder();

            Assert.That(viewFolder.HasView(Path.Combine("Home", "Index.spark")), Is.False);
            viewFolder.Add(Path.Combine("Home", "Index.spark"), "stuff");
            Assert.Multiple(() =>
            {
                Assert.That(viewFolder.HasView(Path.Combine("Home", "Index.spark")), Is.True);
                Assert.That(viewFolder.HasView(Path.Combine("Home", "Index")), Is.False);
                Assert.That(viewFolder.HasView(Path.Combine("Home", "index.spark")), Is.True);
                Assert.That(viewFolder.HasView(Path.Combine("home", "INDEX.SPARK")), Is.True);
            });
        }

        [Test]
        public void ListViewsInFolder()
        {
            var viewFolder = new InMemoryViewFolder
            {
                { Path.Combine("Home", "Alpha.spark"), "stuff" },
                { Path.Combine("Home", "Beta.spark"), "stuff" },
                { Path.Combine("Home2", "Gamma.spark"), "stuff" },
                { Path.Combine("home", "Delta.spark"), "stuff" },
                { Path.Combine("Home", "Something", "else.spark"), "stuff" }
            };

            var views = viewFolder.ListViews("Home");

            var baseNames = views
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();

            Assert.That(baseNames.Count(), Is.EqualTo(3));
            Assert.That(baseNames, Does.Contain("Alpha"));
            Assert.That(baseNames, Does.Contain("Beta"));
            Assert.That(baseNames, Does.Contain("Delta"));
        }

        [Test]
        public void FileNotFoundException()
        {
            var viewFolder = new InMemoryViewFolder
            {
                { Path.Combine("Home", "Index.spark"), "stuff" }
            };

            Assert.That(() => viewFolder.GetViewSource(Path.Combine("Home", "List.spark")),
                        Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void ReadFileContents()
        {
            var viewFolder = new InMemoryViewFolder
            {
                { Path.Combine("Home", "Index.spark"), "this is the file contents" }
            };

            var source = viewFolder.GetViewSource(Path.Combine("Home", "Index.spark"));
            using (var stream = source.OpenViewStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    var contents = reader.ReadToEnd();
                    Assert.That(contents, Is.EqualTo("this is the file contents"));
                }
            }
        }

        [Test]
        public void LastModifiedChanges()
        {
            var viewFolder = new InMemoryViewFolder
            {
                { Path.Combine("Home", "Index.spark"), "this is the file contents" }
            };

            var source1 = viewFolder.GetViewSource(Path.Combine("Home", "Index.spark"));
            var lastModified1 = source1.LastModified;

            viewFolder.Set(Path.Combine("Home", "Index.spark"), "this is the file contents");
            var source2 = viewFolder.GetViewSource(Path.Combine("Home", "Index.spark"));
            var lastModified2 = source2.LastModified;

            Assert.That(lastModified2, Is.Not.EqualTo(lastModified1));

            var lastModified1b = source1.LastModified;
            var lastModified2b = source1.LastModified;

            Assert.That(lastModified1b, Is.Not.EqualTo(lastModified1));
            Assert.That(lastModified2b, Is.EqualTo(lastModified1b));
        }

        [Test]
        public void InMemoryViewFolderUsedByEngine()
        {
            var viewFolder = new InMemoryViewFolder
            {
                { Path.Combine("home", "index.spark"), "<p>Hello world</p>" }
            };

            var settings = new SparkSettings().SetBaseClassTypeName(typeof(StubSparkView));

            var sp = new ServiceCollection()
                .AddSpark(settings)
                .AddSingleton<IViewFolder>(viewFolder)
                .BuildServiceProvider();

            var engine = sp.GetService<ISparkViewEngine>();

            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add(Path.Combine("home", "index.spark"));
            var view = engine.CreateInstance(descriptor);
            var contents = view.RenderView();
            Assert.That(contents, Is.EqualTo("<p>Hello world</p>"));
        }

        private static string ReadToEnd(IViewFolder viewFolder, string path)
        {
            using (var stream = viewFolder.GetViewSource(path).OpenViewStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static string RenderView(ISparkViewEngine engine, string path)
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
            var viewFolder = new InMemoryViewFolder
            {
                { Path.Combine("Home", "fr.spark"), "Fran\u00E7ais" },
                { Path.Combine("Home", "ru.spark"), "\u0420\u0443\u0441\u0441\u043A\u0438\u0439" },
                { Path.Combine("Home", "ja.spark"), "\u65E5\u672C\u8A9E" }
            };

            Assert.Multiple(() =>
            {
                Assert.That(ReadToEnd(viewFolder, Path.Combine("Home", "fr.spark")), Is.EqualTo("Français"));
                Assert.That(ReadToEnd(viewFolder, Path.Combine("Home", "ru.spark")), Is.EqualTo("Русский"));
                Assert.That(ReadToEnd(viewFolder, Path.Combine("Home", "ja.spark")), Is.EqualTo("日本語"));
            });

            var settings = new SparkSettings().SetBaseClassTypeName(typeof(StubSparkView));

            var sp = new ServiceCollection()
                .AddSpark(settings)
                .AddSingleton<IViewFolder>(viewFolder)
                .BuildServiceProvider();

            var engine = sp.GetService<ISparkViewEngine>();

            Assert.Multiple(() =>
            {
                Assert.That(RenderView(engine, Path.Combine("Home", "fr.spark")), Is.EqualTo("Français"));
                Assert.That(RenderView(engine, Path.Combine("Home", "ru.spark")), Is.EqualTo("Русский"));
                Assert.That(RenderView(engine, Path.Combine("Home", "ja.spark")), Is.EqualTo("日本語"));
            });
        }
    }
}
