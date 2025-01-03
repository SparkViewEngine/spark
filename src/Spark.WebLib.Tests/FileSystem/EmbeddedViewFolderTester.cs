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
using System.Reflection;
using NUnit.Framework;

namespace Spark.FileSystem
{
    [TestFixture]
    public class EmbeddedViewFolderTester
    {
        [Test]
        public void LocateEmbeddedFiles()
        {
            var viewFolder = new EmbeddedViewFolder(Assembly.Load("Spark.Web.Tests"), "Spark.FileSystem.Embedded");
            Assert.That(viewFolder.HasView(Path.Combine("Home", "Index.spark")), Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(viewFolder.HasView(Path.Combine("Home", "NoSuchFile.spark")), Is.False);
                Assert.That(viewFolder.HasView("Home"), Is.False);
            });
            Assert.That(viewFolder.HasView(Path.Combine("Shared", "Default.spark")), Is.True);
        }

        [Test]
        public void ListViewsSameResults()
        {
            var filesystem = new FileSystemViewFolder(Path.Combine("FileSystem", "Embedded"));
            Assert.That(filesystem.HasView(Path.Combine("Home", "Index.spark")), Is.True);

            var files = filesystem.ListViews("Home");
            Assert.That(files.Count, Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(files.Any(f => Path.GetFileName(f) == "Index.spark"));
                Assert.That(files.Any(f => Path.GetFileName(f) == "List.spark"));
            });

            var embedded = new EmbeddedViewFolder(Assembly.Load("Spark.Web.Tests"), "Spark.FileSystem.Embedded");
            files = embedded.ListViews("home");
            Assert.That(files.Count, Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(files.Any(f => Path.GetFileName(f) == "Index.spark"));
                Assert.That(files.Any(f => Path.GetFileName(f) == "List.spark"));
            });
        }
    }
}
