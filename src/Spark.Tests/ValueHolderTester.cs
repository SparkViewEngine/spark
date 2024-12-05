﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;


namespace Spark.Tests
{
    [TestFixture]
    public class ValueHolderTester
    {
        [Test]
        public void AcquireMethodShouldBeCalledOnlyOnce()
        {
            var calls = 0;
            var holder = ValueHolder.For(() =>
                                             {
                                                 ++calls;
                                                 return "hello";
                                             });

            Assert.Multiple(() =>
            {
                Assert.That(calls, Is.EqualTo(0));
                Assert.That(holder.Value, Is.EqualTo("hello"));
            });
            Assert.Multiple(() =>
            {
                Assert.That(holder.Value, Is.EqualTo("hello"));
                Assert.That(calls, Is.EqualTo(1));
            });
        }

        [Test]
        public void KeyPassesThrough()
        {
            var calls = 0;
            var holder = ValueHolder.For("hello", k =>
                                                      {
                                                          ++calls;
                                                          return new string(k.Reverse().ToArray());
                                                      });

            Assert.Multiple(() =>
            {
                Assert.That(calls, Is.EqualTo(0));
                Assert.That(holder.Value, Is.EqualTo("olleh"));
            });
            Assert.Multiple(() =>
            {
                Assert.That(holder.Value, Is.EqualTo("olleh"));
                Assert.That(calls, Is.EqualTo(1));
            });
        }
    }
}
