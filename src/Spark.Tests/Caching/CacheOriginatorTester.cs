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
        private ICacheSubject _subject;
        private CacheOriginator _originator;

        private ICacheSubject _subject2;
        private CacheOriginator _originator2;

        public class TestSubject : ICacheSubject
        {
            public TestSubject()
            {
                Output = new SpoolWriter();
                Content = new Dictionary<string, TextWriter>();
            }
            public TextWriter Output { get; set; }
            public Dictionary<string, TextWriter> Content { get; set; }
        }

        [SetUp]
        public void Init()
        {
            _subject = new TestSubject();
            _originator = new CacheOriginator(_subject);

            _subject2 = new TestSubject();
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

            var subject3 = new TestSubject();

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

        [Test, Ignore("Not implemented yet")]
        public void OnceCollectionExtendedWhenApplied()
        {
            throw new NotImplementedException();
        }

        [Test, Ignore("Not implemented yet")]
        public void OutputWhileNamedContentActiveShouldAppearOnceAtCorrectTarget()
        {
            throw new NotImplementedException();
        }
    }
}
