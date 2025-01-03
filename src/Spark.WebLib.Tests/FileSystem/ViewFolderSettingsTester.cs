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

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace Spark.FileSystem
{
    [TestFixture]
    public class ViewFolderSettingsTester
    {
        [Test]
        public void ApplySettings()
        {
            var settings =
                new SparkSettings()
                    .AddViewFolder(
                        typeof(VirtualPathProviderViewFolder),
                        new Dictionary<string, string> { { "virtualBaseDir", "~/MoreViews/" } });

            var engine = new SparkViewEngine(settings, null, null, null, null, null, null, null, null, null, null);

            var folder = engine.ViewFolder;

            Assert.That(folder, Is.AssignableFrom(typeof(CombinedViewFolder)));

            var combined = (CombinedViewFolder)folder;
            Assert.That(combined.Second, Is.AssignableFrom(typeof(VirtualPathProviderViewFolder)));

            var vpp = (VirtualPathProviderViewFolder)combined.Second;
            Assert.That(vpp.VirtualBaseDir, Is.EqualTo("~/MoreViews/"));
        }

        [Test]
        public void CustomViewFolder()
        {
            var settings = new SparkSettings()
                .AddViewFolder(
                    typeof(MyViewFolder),
                    new Dictionary<string, string> { { "foo", "quux" }, { "bar", "42" } });

            var engine = new SparkViewEngine(settings, null, null, null, null, null, null, null, null, null, null);

            var folder = engine.ViewFolder;
            Assert.That(folder, Is.AssignableFrom(typeof(CombinedViewFolder)));
            var combined = (CombinedViewFolder)folder;
            Assert.That(combined.Second, Is.AssignableFrom(typeof(MyViewFolder)));
            var customFolder = (MyViewFolder)combined.Second;
            Assert.That(customFolder.Foo, Is.EqualTo("quux"));
            Assert.That(customFolder.Bar, Is.EqualTo(42));
        }

        [Test]
        public void AssemblyParameter()
        {
            var settings = new SparkSettings()
                .AddViewFolder(
                    typeof(EmbeddedViewFolder),
                    new Dictionary<string, string>
                        { { "assembly", "Spark.Tests" }, { "resourcePath", "Spark.Tests.Views" } });

            var engine = new SparkViewEngine(settings, null, null, null, null, null, null, null, null, null, null);

            var folder = engine.ViewFolder;
            Assert.That(folder, Is.AssignableFrom(typeof(CombinedViewFolder)));
            var combined = (CombinedViewFolder)folder;
            Assert.That(combined.Second, Is.AssignableFrom(typeof(EmbeddedViewFolder)));
            var embeddedViewFolder = (EmbeddedViewFolder)combined.Second;
            Assert.That(embeddedViewFolder.Assembly, Is.EqualTo(Assembly.Load("Spark.Tests")));
        }

        [Test]
        public void TypeFileSystemCreatesFileSystemViewFolder()
        {
            var settings = new SparkSettings()
                .AddViewFolder(
                    typeof(FileSystemViewFolder),
                    new Dictionary<string, string>
                    {
                        { "basePath", @"e:\no\such\path" }
                    });

            var engine = new SparkViewEngine(settings, null, null, null, null, null, null, null, null, null, null);
            var folder = engine.ViewFolder;
            Assert.That(folder, Is.AssignableFrom(typeof(CombinedViewFolder)));
            var combined = (CombinedViewFolder)folder;
            Assert.That(combined.Second, Is.AssignableFrom(typeof(FileSystemViewFolder)));
            var fileSystemViewFolder = (FileSystemViewFolder)combined.Second;
            Assert.That(fileSystemViewFolder.BasePath, Is.EqualTo(@"e:\no\such\path"));
        }

        public class MyViewFolder : IViewFolder
        {
            public string Foo { get; set; }
            public int Bar { get; set; }

            public MyViewFolder(string foo, int bar)
            {
                Foo = foo;
                Bar = bar;
            }

            public IViewFile GetViewSource(string path)
            {
                throw new System.NotImplementedException();
            }

            public IList<string> ListViews(string path)
            {
                throw new System.NotImplementedException();
            }

            public bool HasView(string path)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
