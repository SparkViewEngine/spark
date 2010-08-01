using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SparkSense.Parsing;
using System.IO;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class CachingViewFolderTests
    {
        private const string ROOT_VIEW_PATH = "SparkSense.Tests.Views";

        [Test]
        public void ShouldLoadFromDiskIfPathNotInCache()
        {
            string contents = String.Empty;
            var cache = new CachingViewFolder(ROOT_VIEW_PATH);
            var content = cache.GetViewSource("Shared\\Application.spark");

            using (TextReader reader = new StreamReader(content.OpenViewStream()))
                contents = reader.ReadToEnd();

            Assert.That(contents.Contains("no header by default"));
        }

        [Test]
        public void ShouldLoadFromDiskIfPathInCacheWithNullData()
        {
            string path = "Shared\\Application.spark";
            string contents = String.Empty;
            var cache = new CachingViewFolder(ROOT_VIEW_PATH);
            cache.Add(path);
            var content = cache.GetViewSource(path);

            using (TextReader reader = new StreamReader(content.OpenViewStream()))
                contents = reader.ReadToEnd();

            Assert.That(contents.Contains("no header by default"));
        }
    }
}
