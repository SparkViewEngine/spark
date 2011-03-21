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
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests.FileSystem
{
    [TestFixture]
    public class ViewFolderSettingsTester
    {
        [Test]
        public void ApplySettings()
        {
            var settings = new SparkSettings()
                .AddViewFolder(ViewFolderType.VirtualPathProvider, new Dictionary<string, string> { { "virtualBaseDir", "~/MoreViews/" } });
            var engine = new SparkViewEngine(settings);

            var folder = engine.ViewFolder;
            Assert.IsAssignableFrom(typeof(CombinedViewFolder), folder);
            var combined = (CombinedViewFolder)folder;
            Assert.IsAssignableFrom(typeof(VirtualPathProviderViewFolder), combined.Second);
            var vpp = (VirtualPathProviderViewFolder)combined.Second;
            Assert.AreEqual("~/MoreViews/", vpp.VirtualBaseDir);
        }

        [Test]
        public void CustomViewFolder()
        {
            var settings = new SparkSettings()
                .AddViewFolder(typeof(MyViewFolder), new Dictionary<string, string> { { "foo", "quux" }, { "bar", "42" } });
            var engine = new SparkViewEngine(settings);

            var folder = engine.ViewFolder;
            Assert.IsAssignableFrom(typeof(CombinedViewFolder), folder);
            var combined = (CombinedViewFolder)folder;
            Assert.IsAssignableFrom(typeof(MyViewFolder), combined.Second);
            var customFolder = (MyViewFolder)combined.Second;
            Assert.AreEqual("quux", customFolder.Foo);
            Assert.AreEqual(42, customFolder.Bar);
        }

        [Test]
        public void AssemblyParameter()
        {
            var settings = new SparkSettings()
                .AddViewFolder(ViewFolderType.EmbeddedResource, new Dictionary<string, string> { { "assembly", "Spark.Tests" }, { "resourcePath", "Spark.Tests.Views" } });

            var engine = new SparkViewEngine(settings);

            var folder = engine.ViewFolder;
            Assert.IsAssignableFrom(typeof(CombinedViewFolder), folder);
            var combined = (CombinedViewFolder)folder;
            Assert.IsAssignableFrom(typeof(EmbeddedViewFolder), combined.Second);
            var embeddedViewFolder = (EmbeddedViewFolder)combined.Second;
            Assert.AreEqual(Assembly.Load("Spark.Tests"), embeddedViewFolder.Assembly);
        }

        [Test]
        public void TypeFileSystemCreatesFileSystemViewFolder()
        {
            var settings = new SparkSettings()
                .AddViewFolder(ViewFolderType.FileSystem, new Dictionary<string, string>
                                                              {
                                                                  {"basePath", @"e:\no\such\path"}
                                                              });
            var engine = new SparkViewEngine(settings);
            var folder = engine.ViewFolder;
            Assert.IsAssignableFrom(typeof(CombinedViewFolder), folder);
            var combined = (CombinedViewFolder)folder;
            Assert.IsAssignableFrom(typeof(FileSystemViewFolder), combined.Second);
            var fileSystemViewFolder = (FileSystemViewFolder)combined.Second;
            Assert.AreEqual(@"e:\no\such\path", fileSystemViewFolder.BasePath);
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
