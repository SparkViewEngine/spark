using System.Linq;
using NUnit.Framework;
using Spark.Spool;

namespace Spark.Caching
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
            this._subject = new SparkViewContext { Output = new SpoolWriter() };
            this._originator = new CacheOriginator(this._subject);

            this._subject2 = new SparkViewContext { Output = new SpoolWriter() };
            this._originator2 = new CacheOriginator(this._subject2);
        }

        [Test]
        public void OriginatorShouldCreateMemento()
        {
            this._originator.BeginMemento();
            var memento = this._originator.EndMemento();
            Assert.That(memento, Is.Not.Null);
        }

        [Test]
        public void ApplyMementoShouldAddTextToStream()
        {
            this._originator.BeginMemento();
            this._subject.Output.Write("Hello");
            this._subject.Output.Write("World");
            var memento = this._originator.EndMemento();

            this._originator2.DoMemento(memento);
            Assert.That(this._subject2.Output.ToString(), Is.EqualTo("HelloWorld"));
        }

        [Test]
        public void AdditionalWritesBeforeAndAfterAreMoot()
        {
            this._subject.Output.Write("Alpha");
            this._originator.BeginMemento();
            this._subject.Output.Write("Beta");
            var memento = this._originator.EndMemento();
            this._subject.Output.Write("Gamma");

            this._subject2.Output.Write("Delta");
            this._originator2.DoMemento(memento);
            this._subject2.Output.Write("Epsilon");

            Assert.Multiple(() =>
            {
                Assert.That(this._subject.Output.ToString(), Is.EqualTo("AlphaBetaGamma"));
                Assert.That(this._subject2.Output.ToString(), Is.EqualTo("DeltaBetaEpsilon"));
            });
        }

        [Test]
        public void DisposingSpoolWritersShouldNotDamageCaches()
        {
            this._subject.Output.Write("Alpha");
            this._originator.BeginMemento();
            this._subject.Output.Write("Beta");
            var memento = this._originator.EndMemento();
            this._subject.Output.Write("Gamma");

            this._subject2.Output.Write("Delta");
            this._originator2.DoMemento(memento);
            this._subject2.Output.Write("Epsilon");

            Assert.Multiple(() =>
            {
                Assert.That(this._subject.Output.ToString(), Is.EqualTo("AlphaBetaGamma"));
                Assert.That(this._subject2.Output.ToString(), Is.EqualTo("DeltaBetaEpsilon"));
            });

            this._subject.Output.Dispose();
            Assert.That(this._subject2.Output.ToString(), Is.EqualTo("DeltaBetaEpsilon"));
            this._subject2.Output.Dispose();

            var subject3 = new SparkViewContext { Output = new SpoolWriter() };

            subject3.Output.Write("Zeta");
            new CacheOriginator(subject3).DoMemento(memento);
            subject3.Output.Write("Eta");
            Assert.That(subject3.Output.ToString(), Is.EqualTo("ZetaBetaEta"));
        }

        [Test]
        public void AddedNamedContentAddedWhenApplied()
        {
            this._originator.BeginMemento();
            this._subject.Content.Add("foo", new SpoolWriter());
            this._subject.Output.Write("alpha");
            this._subject.Content["foo"].Write("beta");
            var memento = this._originator.EndMemento();

            this._originator2.DoMemento(memento);

            Assert.Multiple(() =>
            {
                Assert.That(this._subject2.Output.ToString(), Is.EqualTo("alpha"));
                Assert.That(this._subject2.Content["foo"].ToString(), Is.EqualTo("beta"));
            });
        }

        [Test]
        public void WrittenNamedContentWrittenWhenApplied()
        {
            this._subject.Content.Add("foo", new SpoolWriter());
            this._subject.Content["foo"].Write("hello");

            this._originator.BeginMemento();
            this._subject.Output.Write("alpha");
            this._subject.Content["foo"].Write("beta");
            var memento = this._originator.EndMemento();

            this._subject2.Content.Add("foo", new SpoolWriter());
            this._subject2.Content["foo"].Write("world");

            this._originator2.DoMemento(memento);

            Assert.Multiple(() =>
            {
                Assert.That(this._subject2.Output.ToString(), Is.EqualTo("alpha"));
                Assert.That(this._subject2.Content["foo"].ToString(), Is.EqualTo("worldbeta"));
            });
        }

        [Test]
        public void OnceCollectionExtendedWhenApplied()
        {
            this._subject.OnceTable.Add("hana", "duul");
            this._originator.BeginMemento();
            this._subject.OnceTable.Add("set", "net");
            var memento = this._originator.EndMemento();
            this._subject.OnceTable.Add("daset", "yaset");

            this._subject2.OnceTable.Add("ilgot", "yadul");
            this._originator2.DoMemento(memento);
            this._subject2.OnceTable.Add("ahop", "yuul");

            Assert.Multiple(() =>
            {
                Assert.That(this._subject.OnceTable.Count(), Is.EqualTo(3));
                Assert.That(this._subject.OnceTable["hana"], Is.EqualTo("duul"));
                Assert.That(this._subject.OnceTable["set"], Is.EqualTo("net"));
                Assert.That(this._subject.OnceTable["daset"], Is.EqualTo("yaset"));

                Assert.That(this._subject2.OnceTable.Count(), Is.EqualTo(3));
                Assert.That(this._subject2.OnceTable["ilgot"], Is.EqualTo("yadul"));
                Assert.That(this._subject2.OnceTable["set"], Is.EqualTo("net"));
                Assert.That(this._subject2.OnceTable["ahop"], Is.EqualTo("yuul"));
            });
        }

        [Test]
        public void OutputWhileNamedContentActiveShouldAppearOnceAtCorrectTarget()
        {
            this._subject.Content.Add("foo", new SpoolWriter());
            this._subject.Content.Add("bar", new SpoolWriter());
            this._subject.Output = this._subject.Content["foo"];
            this._subject.Output.Write("hello");

            this._originator.BeginMemento();
            this._subject.Content["foo"].Write(" ");
            this._subject.Content["bar"].Write("yadda");
            this._subject.Output.Write("world");
            this._subject.Content["foo"].Write("!");
            var memento = this._originator.EndMemento();

            Assert.Multiple(() =>
            {
                Assert.That(this._subject.Output.ToString(), Is.EqualTo("hello world!"));
                Assert.That(this._subject.Content["foo"].ToString(), Is.EqualTo("hello world!"));
            });

            this._subject2.Content.Add("foo", new SpoolWriter());
            this._subject2.Output = this._subject2.Content["foo"];
            this._subject2.Output.Write("hello");
            this._originator2.DoMemento(memento);

            Assert.Multiple(() =>
            {
                Assert.That(this._subject2.Output.ToString(), Is.EqualTo("hello world!"));
                Assert.That(this._subject2.Content["foo"].ToString(), Is.EqualTo("hello world!"));
            });
        }
    }
}
