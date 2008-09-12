using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests.FileSystem
{
    [TestFixture]
    public class CombinedViewFolderTester
    {
        [Test]
        public void HasTemplate()
        {
            var first = new InMemoryViewFolder { { "one.txt", "one" } };
            var second = new InMemoryViewFolder { { "two.txt", "two" } };
            var viewFolder = new CombinedViewFolder(first, second);

            Assert.IsTrue(viewFolder.HasView("one.txt"));
            Assert.IsTrue(viewFolder.HasView("two.txt"));
            Assert.IsFalse(viewFolder.HasView("three.txt"));
        }

        [Test]
        public void OpenFileStream()
        {
            var first = new InMemoryViewFolder { { "one.txt", "one" } };
            var second = new InMemoryViewFolder { { "two.txt", "two" } };
            var viewFolder = new CombinedViewFolder(first, second);

            using (var reader = new StreamReader(viewFolder.GetViewSource("one.txt").OpenViewStream()))
            {
                var content = reader.ReadToEnd();
                Assert.AreEqual("one", content);
            }

            using (var reader = new StreamReader(viewFolder.GetViewSource("two.txt").OpenViewStream()))
            {
                var content = reader.ReadToEnd();
                Assert.AreEqual("two", content);
            }
        }

        [Test, ExpectedException(typeof(FileNotFoundException))]
        public void OpenMissingFile()
        {
            var first = new InMemoryViewFolder { { "one.txt", "one" } };
            var second = new InMemoryViewFolder { { "two.txt", "two" } };
            var viewFolder = new CombinedViewFolder(first, second);

            viewFolder.GetViewSource("three.txt");
        }


        [Test]
        public void OpenFromFirstViewFolder()
        {
            var first = new InMemoryViewFolder { { "one.txt", "one" } };
            var second = new InMemoryViewFolder { { "one.txt", "two" } };
            var viewFolder = new CombinedViewFolder(first, second);

            using (var reader = new StreamReader(viewFolder.GetViewSource("one.txt").OpenViewStream()))
            {
                var content = reader.ReadToEnd();
                Assert.AreEqual("one", content);
            }
        }

        [Test]
        public void ListFilesWithDedupe()
        {
            var first = new InMemoryViewFolder { { "home\\three.txt", "three" }, { "home\\one.txt", "one" } };
            var second = new InMemoryViewFolder { { "home\\two.txt", "two" }, { "home\\three.txt", "three" } };
            var viewFolder = new CombinedViewFolder(first, second);

            var views = viewFolder.ListViews("home");
            Assert.AreEqual(3, views.Count);
            Assert.Contains("home\\one.txt", views.ToArray());
            Assert.Contains("home\\two.txt", views.ToArray());
            Assert.Contains("home\\three.txt", views.ToArray());
        }

    }
}
