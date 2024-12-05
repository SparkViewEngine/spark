using System;
using NUnit.Framework;

namespace Spark.Caching
{
    [TestFixture]
    public class CacheSignalTester
    {
        [Test]
        public void SignalCanBeFiredExternally()
        {
            var fired = false;
            var signal = new CacheSignal();
            signal.Changed += (sender, e) => { fired = true; };

            Assert.That(fired, Is.False);
            signal.FireChanged();
            Assert.That(fired, Is.True);
        }

        [Test]
        public void UnusedSignalCanBeFiredSafely()
        {
            var signal = new CacheSignal();
            signal.FireChanged();
            signal.Changed += ignore_signal_event_handler;
            signal.FireChanged();
            signal.Changed -= ignore_signal_event_handler;
            signal.FireChanged();
        }

        static void ignore_signal_event_handler(object sender, EventArgs e)
        {
        }

        static void ignore_signal_event_handler2(object sender, EventArgs e)
        {
        }

        [Test]
        public void EnableAndDisableShouldBeCalledAsEventHandlerCountTransitionsBetweenZeroAndNonZero()
        {
            var signal = new EnableTestingSignal();

            Assert.That(signal.Enabled, Is.False);
            signal.FireChanged();
            Assert.That(signal.Enabled, Is.False);

            Assert.That(signal.Enabled, Is.False);
            signal.Changed += ignore_signal_event_handler;
            Assert.That(signal.Enabled, Is.True);
            signal.Changed -= ignore_signal_event_handler;
            Assert.That(signal.Enabled, Is.False);

            signal.Changed += ignore_signal_event_handler;
            Assert.That(signal.Enabled, Is.True);
            signal.Changed += ignore_signal_event_handler2;
            Assert.That(signal.Enabled, Is.True);
            signal.Changed -= ignore_signal_event_handler;
            Assert.That(signal.Enabled, Is.True);
            signal.Changed -= ignore_signal_event_handler2;
            Assert.That(signal.Enabled, Is.False);
        }

        class EnableTestingSignal : CacheSignal
        {
            protected override void Enable()
            {
                Assert.That(this.Enabled, Is.False);
                this.Enabled = true;
            }
            protected override void Disable()
            {
                Assert.That(this.Enabled, Is.True);
                this.Enabled = false;
            }

            public bool Enabled { get; set; }
        }
    }
}
