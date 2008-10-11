using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Spark.Spool;

namespace Spark.Tests.Spool
{
    [TestFixture]
    public class SpoolWriterTester
    {
        private string _FiveThousandNumbers;
        private Stack<string[]> _cache;

        [TestFixtureSetUp]
        public void InitOnce()
        {
            var sb = new StringBuilder();
            for (int index = 0; index != 5000; ++index)
                sb.Append(index);
            _FiveThousandNumbers = sb.ToString();
        }

        [SetUp]
        public void Init()
        {
            var allocatorField = typeof(SpoolPage).GetField("_allocator", BindingFlags.Static | BindingFlags.NonPublic);
            var allocator = allocatorField.GetValue(null);

            var cacheField = allocator.GetType().GetField("_cache", BindingFlags.Instance | BindingFlags.NonPublic);
            _cache = (Stack<string[]>)cacheField.GetValue(allocator);

            // finalize any spoolwriters and zero out string page pool
            GC.Collect();
            GC.WaitForPendingFinalizers();
            _cache.Clear();
        }

        [Test]
        public void ToStringCombinesResults()
        {
            TextWriter writer = new SpoolWriter();
            writer.Write("hello");
            writer.Write("world");
            Assert.AreEqual("helloworld", writer.ToString());
        }

        [Test]
        public void MultiplePagesCombinedResult()
        {
            TextWriter writer = new SpoolWriter();
            for (int index = 0; index != 5000; ++index)
            {
                writer.Write(index);
            }
            Assert.AreEqual(_FiveThousandNumbers, writer.ToString());
        }

        [Test]
        public void StringWriterToSpoolWriter()
        {
            TextWriter writer1 = new StringWriter();
            TextWriter writer2 = new SpoolWriter();
            for (int index = 0; index != 5000; ++index)
            {
                writer1.Write(index);
            }
            writer2.Write("before");
            writer1.WriteTo(writer2);
            writer2.Write("after");
            Assert.AreEqual("before" + _FiveThousandNumbers + "after", writer2.ToString());
        }

        [Test]
        public void SpoolWriterToStringWriter()
        {
            TextWriter writer1 = new SpoolWriter();
            TextWriter writer2 = new StringWriter();
            for (int index = 0; index != 5000; ++index)
            {
                writer1.Write(index);
            }
            writer2.Write("before");
            writer1.WriteTo(writer2);
            writer2.Write("after");
            Assert.AreEqual("before" + _FiveThousandNumbers + "after", writer2.ToString());
        }

        [Test]
        public void SpoolWriterToSpoolWriter()
        {
            TextWriter writer1 = new SpoolWriter();
            TextWriter writer2 = new SpoolWriter();
            for (int index = 0; index != 5000; ++index)
            {
                writer1.Write(index);
            }
            writer2.Write("before");
            writer1.WriteTo(writer2);
            writer2.Write("after");
            Assert.AreEqual("before" + _FiveThousandNumbers + "after", writer2.ToString());
        }

        [Test]
        public void AppendingOwnsBuffer()
        {
            var countBefore = _cache.Count;

            TextWriter writer1 = new SpoolWriter();
            writer1.Write("hello");

            TextWriter writer2 = new SpoolWriter();
            writer2.Write("before");
            writer2.Write(writer1);
            writer2.Write("after");

            writer1.Write("world");

            Assert.AreEqual("helloworld", writer1.ToString());
            Assert.AreEqual("beforehelloafter", writer2.ToString());

            var _first = typeof(SpoolWriter).GetField("_first", BindingFlags.NonPublic | BindingFlags.Instance);
            var _readonly = typeof(SpoolPage).GetField("_readonly", BindingFlags.NonPublic | BindingFlags.Instance);
            var _nonreusable = typeof(SpoolPage).GetField("_nonreusable", BindingFlags.NonPublic | BindingFlags.Instance);

            var pages1 = (SpoolPage)_first.GetValue(writer1);
            var pages2 = (SpoolPage)_first.GetValue(writer2);

            Assert.IsTrue((bool)_readonly.GetValue(pages1));
            Assert.IsTrue((bool)_nonreusable.GetValue(pages1));

            Assert.IsFalse((bool)_readonly.GetValue(pages1.Next));
            Assert.IsFalse((bool)_nonreusable.GetValue(pages1.Next));
            Assert.IsNull(pages1.Next.Next);


            Assert.IsTrue((bool)_readonly.GetValue(pages2));
            Assert.IsFalse((bool)_nonreusable.GetValue(pages2));

            Assert.IsTrue((bool)_readonly.GetValue(pages2.Next));
            Assert.IsTrue((bool)_nonreusable.GetValue(pages2.Next));

            Assert.IsFalse((bool)_readonly.GetValue(pages2.Next.Next));
            Assert.IsFalse((bool)_nonreusable.GetValue(pages2.Next.Next));

            Assert.IsNull(pages2.Next.Next.Next);

            var countBeforeCollect = _cache.Count;
            // ReSharper disable RedundantAssignment
            writer1 = null;
            writer2 = null;
            // ReSharper restore RedundantAssignment
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var countAfterCollect = _cache.Count;

            Assert.AreEqual(0, countBefore);
            Assert.AreEqual(0, countBeforeCollect);
            Assert.AreEqual(3, countAfterCollect);
        }

        [Test]
        public void WritingCharacters()
        {
            TextWriter writer = new SpoolWriter();
            writer.Write('a');
            writer.Write(new[] { 'b', 'c', 'd' });
            writer.Write(new[] { 'x', 'e', 'f', 'g', 'x' }, 1, 3);
            Assert.AreEqual("abcdefg", writer.ToString());
        }

        [Test]
        public void DisposingWriter()
        {
            var countBefore = _cache.Count;
            using (TextWriter writer = new SpoolWriter())
            {
                writer.Write("sending to the pool");
            }
            var countBetween = _cache.Count;
            using (TextWriter writer = new SpoolWriter())
            {
                writer.Write("taking and returning to pool");
            }

            // force GC to ensure finalize doesn't push buffers into allocator again
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var countAfter = _cache.Count;

            Assert.AreEqual(0, countBefore);
            Assert.AreEqual(1, countBetween);
            Assert.AreEqual(1, countAfter);
        }

    }
}
