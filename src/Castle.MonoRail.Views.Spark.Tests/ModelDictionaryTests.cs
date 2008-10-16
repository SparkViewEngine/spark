using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Castle.MonoRail.Views.Spark.Tests
{
    [TestFixture]
    public class ModelDictionaryTests
    {
        [Test]
        public void AnonymousObjectPropertiesInDictionary()
        {
            IDictionary<string, object> args = new ModelDictionary(new {name = "foo", bar = "quux"});
            Assert.AreEqual(2, args.Count);
            Assert.AreEqual("foo", args["name"]);
            Assert.AreEqual("quux", args["bar"]);
        }
    }
}
