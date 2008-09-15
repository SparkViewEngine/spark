using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Compiler;

namespace Spark.Tests.Compiler
{
    [TestFixture]
    public class ForEachInspectorTester
    {
        [Test]
        public void LocatingTheVariable()
        {
            var inspector = new ForEachInspector("my var thing in collection.thing foo");
            Assert.AreEqual("my var", inspector.VariableType); 
            Assert.AreEqual("thing", inspector.VariableName);
            Assert.AreEqual("collection.thing foo", inspector.CollectionCode);
        }
    }
}
