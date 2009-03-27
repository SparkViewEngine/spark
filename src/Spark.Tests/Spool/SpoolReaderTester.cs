using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.Spool;

namespace Spark.Tests.Spool
{
    [TestFixture]
    public class SpoolReaderTester
    {
        [Test]
        public void StringReader_and_SpoolReader_should_return_minus_one_if_empty()
        {
            var reader1 = new StringReader("");
            var reader2 = new SpoolReader(new SpoolWriter());

            Assert.That(reader1.Peek(), Is.EqualTo(-1));
            Assert.That(reader2.Peek(), Is.EqualTo(-1));
        }

        [Test]
        public void StringReader_should_return_written_data()
        {
            var writer = new SpoolWriter();
            writer.Write("Hello world");

            var reader = new SpoolReader(writer);
            var content = reader.ReadToEnd();

            Assert.That(content, Is.EqualTo("Hello world"));
        }

        [Test]
        public void Multiple_writes_are_combined()
        {
            var writer = new SpoolWriter();
            writer.Write("Hello");
            writer.Write(' ');
            writer.Write("World");

            var reader = new SpoolReader(writer);
            var content = reader.ReadToEnd();

            Assert.That(content, Is.EqualTo("Hello World"));
        }

        [Test]
        public void Peek_and_read_return_characters_and_advance_appropriately()
        {
            var writer = new SpoolWriter();
            writer.Write("ab");
            writer.Write("c");

            var reader = new SpoolReader(writer);
            Assert.That(reader.Peek(), Is.EqualTo((int)'a'));
            Assert.That(reader.Read(), Is.EqualTo((int)'a'));
            Assert.That(reader.Peek(), Is.EqualTo((int)'b'));
            Assert.That(reader.Read(), Is.EqualTo((int)'b'));
            Assert.That(reader.Peek(), Is.EqualTo((int)'c'));
            Assert.That(reader.Read(), Is.EqualTo((int)'c'));
            Assert.That(reader.Peek(), Is.EqualTo(-1));
            Assert.That(reader.Read(), Is.EqualTo(-1));
        }
        
        [Test]
        public void Empty_and_null_writes_are_acceptable()
        {
            var writer = new SpoolWriter();
            writer.Write("a");
            writer.Write("");
            writer.Write((string)null);
            writer.Write("b");

            var reader = new SpoolReader(writer);
            var content = reader.ReadToEnd();
            Assert.That(content, Is.EqualTo("ab"));
        }
    }
}
