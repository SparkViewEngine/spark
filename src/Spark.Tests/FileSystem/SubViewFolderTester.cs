using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests.FileSystem
{
    [TestFixture]
    public class SubViewFolderTester
    {
        [Test]
        public void SharingExtraFolders()
        {
            var normal = new FileSystemViewFolder("Spark.Tests.Views");
            var otherLocation = new FileSystemViewFolder("Spark.Tests.Views\\Prefix");

            var viewFolder = new CombinedViewFolder(normal, new SubViewFolder(otherLocation, "Shared"));

            var normalSharedCount = normal.ListViews("Shared").Count;
            var otherLocationCount = otherLocation.ListViews("").Count;
            var totalSharedCount = viewFolder.ListViews("Shared").Count;

            Assert.AreEqual(normalSharedCount + otherLocationCount, totalSharedCount);
        }
    }
}
