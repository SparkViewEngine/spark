using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.Utilities;

namespace Spark.Tests.Caching
{
    [TestFixture]
    public class CacheUtilitiesTester
    {
        [Test]
        public void KeyConcatinationShouldSimplyStringConcatWithZeroOrOneParts()
        {
            var id0 = CacheUtilities.ToIdentifier("foo", new object[] { });
            Assert.That(id0, Is.EqualTo("foo"));

            var id1a = CacheUtilities.ToIdentifier("foo", new object[] { "bar" });
            Assert.That(id1a, Is.EqualTo("foobar"));

            var id1b = CacheUtilities.ToIdentifier("foo", new object[] { 45.2 });
            Assert.That(id1b, Is.EqualTo("foo45.2"));
        }

        [Test]
        public void KeyMultipleKeysHaveUnitSeperatorDelimitingParts()
        {
            var id2 = CacheUtilities.ToIdentifier("foo", new object[] { "bar", "quux" });
            Assert.That(id2, Is.EqualTo("foobar\u001fquux"));

            var id3 = CacheUtilities.ToIdentifier("foo", new object[] { 45.2, null, this });
            Assert.That(id3, Is.EqualTo("foo45.2\u001f\u001fSpark.Tests.Caching.CacheUtilitiesTester"));
        }
    }
}
