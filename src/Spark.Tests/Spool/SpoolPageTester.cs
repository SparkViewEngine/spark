using NUnit.Framework;
using Spark.Spool;

namespace Spark.Tests.Spool
{
    [TestFixture]
    public class SpoolPageTester
    {
        [Test]
        public void Appending()
        {
            var page = new SpoolPage();
            page.Append("hello");
            Assert.IsNull(page.Next);
            Assert.AreEqual(1, page.Count);
            Assert.AreEqual("hello", page.Buffer[0]);
        }

        [Test]
        public void AppendMultiple()
        {
            var page = new SpoolPage();
            page.Append("hello");
            page.Append("world");
            Assert.IsNull(page.Next);
            Assert.AreEqual(2, page.Count);
            Assert.AreEqual("hello", page.Buffer[0]);
            Assert.AreEqual("world", page.Buffer[1]);
        }

        [Test]
        public void AppendOverBoundary()
        {
            var page = new SpoolPage();
            var last = page;
            for (int index = 0; index != SpoolPage.BUFFER_SIZE + 30; ++index)
            {
                last = last.Append(index.ToString());
            }
            Assert.AreNotSame(page, last);
            Assert.AreEqual(SpoolPage.BUFFER_SIZE, page.Count);
            Assert.AreEqual(30, last.Count);
            Assert.AreEqual(SpoolPage.BUFFER_SIZE.ToString(), last.Buffer[0]);
        }
    }
}
