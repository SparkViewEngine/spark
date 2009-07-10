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
    public class CacheOriginatorTester
    {
        private SparkViewContext _subject;
        private CacheOriginator _originator;

        private SparkViewContext _subject2;
        private CacheOriginator _originator2;


        [SetUp]
        public void Init()
        {
            _subject = new SparkViewContext {Output = new SpoolWriter()};
            _originator = new CacheOriginator(_subject);

            _subject2 = new SparkViewContext { Output = new SpoolWriter() };
            _originator2 = new CacheOriginator(_subject2);
        }

        [Test]
        public void OriginatorShouldCreateMemento()
        {
            _originator.BeginMemento();
            var memento = _originator.EndMemento();
            Assert.That(memento, Is.Not.Null);
        }

        [Test]
        public void ApplyMementoShouldAddTextToStream()
        {
            _originator.BeginMemento();
            _subject.Output.Write("Hello");
            _subject.Output.Write("World");            
            var memento = _originator.EndMemento();

            _originator2.DoMemento(memento);
            Assert.That(_subject2.Output.ToString(), Is.EqualTo("HelloWorld"));            
        }

        [Test]
        public void AdditionalWritesBeforeAndAfterAreMoot()
        {
            _subject.Output.Write("Alpha");
            _originator.BeginMemento();
            _subject.Output.Write("Beta");
            var memento = _originator.EndMemento();
            _subject.Output.Write("Gamma");

            _subject2.Output.Write("Delta");
            _originator2.DoMemento(memento);
            _subject2.Output.Write("Epsilon");

            Assert.That(_subject.Output.ToString(), Is.EqualTo("AlphaBetaGamma"));
            Assert.That(_subject2.Output.ToString(), Is.EqualTo("DeltaBetaEpsilon"));
        }

        [Test]
        public void DisposingSpoolWritersShouldNotDamageCaches()
        {
            _subject.Output.Write("Alpha");
            _originator.BeginMemento();
            _subject.Output.Write("Beta");
            var memento = _originator.EndMemento();
            _subject.Output.Write("Gamma");

            _subject2.Output.Write("Delta");
            _originator2.DoMemento(memento);
            _subject2.Output.Write("Epsilon");

            Assert.That(_subject.Output.ToString(), Is.EqualTo("AlphaBetaGamma"));
            Assert.That(_subject2.Output.ToString(), Is.EqualTo("DeltaBetaEpsilon"));

            _subject.Output.Dispose();
            Assert.That(_subject2.Output.ToString(), Is.EqualTo("DeltaBetaEpsilon"));
            _subject2.Output.Dispose();

            var subject3 = new SparkViewContext { Output = new SpoolWriter() };

            subject3.Output.Write("Zeta"); 
            new CacheOriginator(subject3).DoMemento(memento);            
            subject3.Output.Write("Eta");
            Assert.That(subject3.Output.ToString(), Is.EqualTo("ZetaBetaEta"));
        }

        [Test]
        public void AddedNamedContentAddedWhenApplied()
        {
            _originator.BeginMemento();
            _subject.Content.Add("foo", new SpoolWriter());
            _subject.Output.Write("alpha");
            _subject.Content["foo"].Write("beta");
            var memento = _originator.EndMemento();

            _originator2.DoMemento(memento);

            Assert.That(_subject2.Output.ToString(), Is.EqualTo("alpha"));
            Assert.That(_subject2.Content["foo"].ToString(), Is.EqualTo("beta"));
        }

        [Test]
        public void WrittenNamedContentWrittenWhenApplied()
        {
            _subject.Content.Add("foo", new SpoolWriter());
            _subject.Content["foo"].Write("hello");

            _originator.BeginMemento();
            _subject.Output.Write("alpha");
            _subject.Content["foo"].Write("beta");
            var memento = _originator.EndMemento();

            _subject2.Content.Add("foo", new SpoolWriter());
            _subject2.Content["foo"].Write("world");

            _originator2.DoMemento(memento);

            Assert.That(_subject2.Output.ToString(), Is.EqualTo("alpha"));
            Assert.That(_subject2.Content["foo"].ToString(), Is.EqualTo("worldbeta"));
        }

        [Test]
        public void OnceCollectionExtendedWhenApplied()
        {
            _subject.OnceTable.Add("hana", "duul");
            _originator.BeginMemento();
            _subject.OnceTable.Add("set", "net");
            var memento = _originator.EndMemento();
            _subject.OnceTable.Add("daset", "yaset");

            _subject2.OnceTable.Add("ilgot", "yadul");
            _originator2.DoMemento(memento);
            _subject2.OnceTable.Add("ahop", "yuul");

            Assert.That(_subject.OnceTable.Count(), Is.EqualTo(3));
            Assert.That(_subject.OnceTable["hana"], Is.EqualTo("duul"));
            Assert.That(_subject.OnceTable["set"], Is.EqualTo("net"));
            Assert.That(_subject.OnceTable["daset"], Is.EqualTo("yaset"));

            Assert.That(_subject2.OnceTable.Count(), Is.EqualTo(3));
            Assert.That(_subject2.OnceTable["ilgot"], Is.EqualTo("yadul"));
            Assert.That(_subject2.OnceTable["set"], Is.EqualTo("net"));
            Assert.That(_subject2.OnceTable["ahop"], Is.EqualTo("yuul"));
        }

        [Test]
        public void OutputWhileNamedContentActiveShouldAppearOnceAtCorrectTarget()
        {
            _subject.Content.Add("foo", new SpoolWriter());
            _subject.Content.Add("bar", new SpoolWriter());
            _subject.Output = _subject.Content["foo"];
            _subject.Output.Write("hello");
            
            _originator.BeginMemento();
            _subject.Content["foo"].Write(" ");
            _subject.Content["bar"].Write("yadda");
            _subject.Output.Write("world");
            _subject.Content["foo"].Write("!");
            var memento = _originator.EndMemento();

            Assert.That(_subject.Output.ToString(), Is.EqualTo("hello world!"));
            Assert.That(_subject.Content["foo"].ToString(), Is.EqualTo("hello world!"));

            _subject2.Content.Add("foo", new SpoolWriter());
            _subject2.Output = _subject2.Content["foo"];
            _subject2.Output.Write("hello");
            _originator2.DoMemento(memento);

            Assert.That(_subject2.Output.ToString(), Is.EqualTo("hello world!"));
            Assert.That(_subject2.Content["foo"].ToString(), Is.EqualTo("hello world!"));
        }
    }
}
