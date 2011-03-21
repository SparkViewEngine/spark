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
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests.FileSystem
{
    [TestFixture]
    public class EmbeddedViewFolderTester
    {
        [Test]
        public void LocateEmbeddedFiles()
        {
            var viewFolder = new EmbeddedViewFolder(Assembly.Load("Spark.Tests"), "Spark.Tests.FileSystem.Embedded");
            Assert.IsTrue(viewFolder.HasView(Path.Combine("Home", "Index.spark")));
            Assert.IsFalse(viewFolder.HasView(Path.Combine("Home", "NoSuchFile.spark")));
            Assert.IsFalse(viewFolder.HasView("Home"));
            Assert.IsTrue(viewFolder.HasView(Path.Combine("Shared", "Default.spark")));
        }

        [Test]
        public void ListViewsSameResults()
        {
            var filesystem = new FileSystemViewFolder(Path.Combine("FileSystem", "Embedded"));
            Assert.IsTrue(filesystem.HasView(Path.Combine("Home", "Index.spark")));

            var files = filesystem.ListViews("Home");
            Assert.AreEqual(2, files.Count);
            Assert.That(files.Any(f => Path.GetFileName(f) == "Index.spark"));
            Assert.That(files.Any(f => Path.GetFileName(f) == "List.spark"));

            var embedded = new EmbeddedViewFolder(Assembly.Load("Spark.Tests"), "Spark.Tests.FileSystem.Embedded");
            files = embedded.ListViews("home");
            Assert.AreEqual(2, files.Count);
            Assert.That(files.Any(f => Path.GetFileName(f) == "Index.spark"));
            Assert.That(files.Any(f => Path.GetFileName(f) == "List.spark"));
        }
    }
}
