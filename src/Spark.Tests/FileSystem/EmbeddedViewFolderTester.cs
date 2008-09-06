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
            Assert.IsTrue(viewFolder.HasView("Home\\Index.spark"));
            Assert.IsFalse(viewFolder.HasView("Home\\NoSuchFile.spark"));
            Assert.IsFalse(viewFolder.HasView("Home"));
            Assert.IsTrue(viewFolder.HasView("Shared\\Default.spark"));
        }

        [Test]
        public void ListViewsSameResults()
        {
            var filesystem = new FileSystemViewFolder("FileSystem\\Embedded");
            Assert.IsTrue(filesystem.HasView("Home\\Index.spark"));

            var files = filesystem.ListViews("home");
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
