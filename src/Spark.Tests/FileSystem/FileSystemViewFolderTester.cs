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
using System.IO;
using System.Threading;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests.FileSystem
{
    [TestFixture]
    public class FileSystemViewFolderTester
    {
        private FileSystemViewFolder _viewFolder;

        [SetUp]
        public void Init()
        {
            _viewFolder = new FileSystemViewFolder("Spark.Tests.Views");
        }

        [Test]
        public void HasViewBoolean()
        {
            var fileExists = _viewFolder.HasView(Path.Combine("Home", "foreach.spark"));
            var fileNotFound = _viewFolder.HasView(Path.Combine("Home", "fakefile.spark"));
            Assert.IsTrue(fileExists);
            Assert.IsFalse(fileNotFound);
        }

        [Test]
        public void ListingViewsInFolder()
        {
            var shared = _viewFolder.ListViews("Shared");
            Assert.That(shared.Contains("_comment.spark"));
            Assert.That(shared.Contains("layout.spark"));
            Assert.That(shared.Contains("partial.spark"));
        }

        [Test, ExpectedException(typeof(FileNotFoundException))]
        public void GetSourceNotFound()
        {
            _viewFolder.GetViewSource(Path.Combine("Home", "NoSuchFile.spark"));
        }


        [Test]
        public void ReadingFileContents()
        {
            var viewSource = _viewFolder.GetViewSource(Path.Combine("Home", "foreach.spark"));
            var reader = new StreamReader(viewSource.OpenViewStream());
            var contents = reader.ReadToEnd();
            Assert.That(contents.Contains("<for each="));
        }

        [Test]
        public void LastModifiedChanges()
        {
            var viewSource = _viewFolder.GetViewSource(Path.Combine("Home", "foreach.spark"));
            var lastModified1 = viewSource.LastModified;

            Thread.Sleep(TimeSpan.FromMilliseconds(75));
            var lastModified2 = viewSource.LastModified;

            Assert.AreEqual(lastModified1, lastModified2);

            File.SetLastWriteTimeUtc(Path.Combine("Spark.Tests.Views","Home","foreach.spark"), DateTime.UtcNow);
            var lastModified3 = viewSource.LastModified;

            Assert.AreNotEqual(lastModified1, lastModified3);
        }
    }
}
