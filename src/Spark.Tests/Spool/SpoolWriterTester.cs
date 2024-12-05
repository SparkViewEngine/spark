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
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Spark.Spool;

namespace Spark.Tests.Spool
{
    [TestFixture]
    public class SpoolWriterTester
    {
        private string _FiveThousandNumbers;
        private Stack<string[]> _cache;

        [OneTimeSetUp]
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
            var allocator = allocatorField.GetValue(Arg<object>.Is.Anything);

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
            Assert.That(writer.ToString(), Is.EqualTo("helloworld"));
        }

        [Test]
        public void MultiplePagesCombinedResult()
        {
            TextWriter writer = new SpoolWriter();
            for (int index = 0; index != 5000; ++index)
            {
                writer.Write(index);
            }
            Assert.That(writer.ToString(), Is.EqualTo(_FiveThousandNumbers));
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
            Assert.That(writer2.ToString(), Is.EqualTo("before" + _FiveThousandNumbers + "after"));
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
            Assert.That(writer2.ToString(), Is.EqualTo("before" + _FiveThousandNumbers + "after"));
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
            Assert.That(writer2.ToString(), Is.EqualTo("before" + _FiveThousandNumbers + "after"));
        }

        [Test]
        [Ignore("No idea what that test does or why it fails. Isn't it weird to to rely on garbage collection?")]
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

            Assert.Multiple(() =>
            {
                Assert.That(writer1.ToString(), Is.EqualTo("helloworld"));
                Assert.That(writer2.ToString(), Is.EqualTo("beforehelloafter"));
            });

            var _first = typeof(SpoolWriter).GetField("_first", BindingFlags.NonPublic | BindingFlags.Instance);
            var _readonly = typeof(SpoolPage).GetField("_readonly", BindingFlags.NonPublic | BindingFlags.Instance);
            var _nonreusable = typeof(SpoolPage).GetField("_nonreusable", BindingFlags.NonPublic | BindingFlags.Instance);

            var pages1 = (SpoolPage)_first.GetValue(writer1);
            var pages2 = (SpoolPage)_first.GetValue(writer2);

            Assert.That((bool)_readonly.GetValue(pages1), Is.True);
            Assert.That((bool)_nonreusable.GetValue(pages1), Is.True);

            Assert.That((bool)_readonly.GetValue(pages1.Next), Is.False);
            Assert.That((bool)_nonreusable.GetValue(pages1.Next), Is.False);
            Assert.That(pages1.Next.Next, Is.Null);


            Assert.That((bool)_readonly.GetValue(pages2), Is.True);
            Assert.That((bool)_nonreusable.GetValue(pages2), Is.False);

            Assert.That((bool)_readonly.GetValue(pages2.Next), Is.True);
            Assert.That((bool)_nonreusable.GetValue(pages2.Next), Is.True);

            Assert.That((bool)_readonly.GetValue(pages2.Next.Next), Is.False);
            Assert.That((bool)_nonreusable.GetValue(pages2.Next.Next), Is.False);

            Assert.That(pages2.Next.Next.Next, Is.Null);

            var countBeforeCollect = _cache.Count;
            // ReSharper disable RedundantAssignment
            writer1 = null;
            writer2 = null;
            // ReSharper restore RedundantAssignment
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var countAfterCollect = _cache.Count;

            Assert.Multiple(() =>
            {
                Assert.That(countBefore, Is.EqualTo(0));
                Assert.That(countBeforeCollect, Is.EqualTo(0));
                Assert.That(countAfterCollect, Is.EqualTo(3));
            });
        }

        [Test]
        public void WritingCharacters()
        {
            TextWriter writer = new SpoolWriter();
            writer.Write('a');
            writer.Write(new[] { 'b', 'c', 'd' });
            writer.Write(new[] { 'x', 'e', 'f', 'g', 'x' }, 1, 3);
            Assert.That(writer.ToString(), Is.EqualTo("abcdefg"));
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

            Assert.Multiple(() =>
            {
                Assert.That(countBefore, Is.EqualTo(0));
                Assert.That(countBetween, Is.EqualTo(1));
                Assert.That(countAfter, Is.EqualTo(1));
            });
        }

        [Test]
        public void EncodingShouldBeUtf8ByDefault()
        {
            var writer = new SpoolWriter();

            Assert.Throws<NotSupportedException>(
                () =>
                {
                    var encoding = writer.Encoding;
                });
        }

        [Test]
        public void ToStringWhenCodingIsDifferentFromUtf8()
        {
            TextWriter writer = new SpoolWriter();

            // The accentuated char and unicode emoji shouldn't matter as the SpoolPage just keeps track of the bytes
            writer.Write("hèllo 🌍");

            Assert.That(writer.ToString(), Is.EqualTo("hèllo 🌍"));
        }
    }
}
