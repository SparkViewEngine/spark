using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Compiler.NodeVisitors;

namespace Spark.Tests.Parser
{
    [TestFixture]
    public class TypeInspectorTester
    {
        [Test]
        public void SimpleFields()
        {
            var result = new TypeInspector("string");
            Assert.AreEqual("string", result.Type);
            Assert.IsNull(result.Name);
        }

        [Test]
        public void Generics()
        {
            var result = new TypeInspector("IList<something>");
            Assert.AreEqual("IList<something>", result.Type);
            Assert.IsNull(result.Name);
        }

        [Test]
        public void GenericsWithName()
        {
            var result = new TypeInspector("IList<something>\r\n\tSomethingList");
            Assert.AreEqual("IList<something>", result.Type);
            Assert.AreEqual("SomethingList", result.Name);
        }

        [Test]
        public void GenericWithSpacesButNoName()
        {
            var result = new TypeInspector("IList<something, int>");
            Assert.AreEqual("IList<something, int>", result.Type);
            Assert.IsNull(result.Name);
        }
    }
}
