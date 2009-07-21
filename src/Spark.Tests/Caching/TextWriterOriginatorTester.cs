using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.Caching;
using Spark.Spool;

namespace Spark.Tests.Caching
{
    [TestFixture]
    public class TextWriterOriginatorTester
    {
        [Test]
        public void StringWriterOriginatorBuildsMementoWithChanges()
        {
            var writer = new StringWriter();
            var originator = TextWriterOriginator.Create(writer);

            writer.Write("Alpha");
            originator.BeginMemento();
            writer.Write("Beta");
            var memento = originator.EndMemento();
            writer.Write("Gamma");

            var writer2 = new StringWriter();
            writer2.Write("Delta");
            TextWriterOriginator.Create(writer2).DoMemento(memento);
            writer2.Write("Epsilon");

            Assert.That(writer.ToString(), Is.EqualTo("AlphaBetaGamma"));
            Assert.That(writer2.ToString(), Is.EqualTo("DeltaBetaEpsilon"));
        }

        [Test]
        public void SpoolWriterOriginatorBuildsMementoWithChanges()
        {
            var writer = new SpoolWriter();
            var originator = TextWriterOriginator.Create(writer);

            writer.Write("Alpha");
            originator.BeginMemento();
            writer.Write("B");
            writer.Write("e");
            writer.Write("t");
            writer.Write("a");
            var memento = originator.EndMemento();
            writer.Write("Gamma");

            var writer2 = new SpoolWriter();
            writer2.Write("Delta");
            TextWriterOriginator.Create(writer2).DoMemento(memento);
            writer2.Write("Epsilon");

            Assert.That(writer.ToString(), Is.EqualTo("AlphaBetaGamma"));
            Assert.That(writer2.ToString(), Is.EqualTo("DeltaBetaEpsilon"));
        }
    }
}
