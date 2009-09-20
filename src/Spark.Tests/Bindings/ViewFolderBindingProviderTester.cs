using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Spark.Bindings;
using Spark.FileSystem;

namespace Spark.Tests.Bindings
{
    [TestFixture]
    public class ViewFolderBindingProviderTester
    {
        [Test]
        public void BindingTableCanLoadFromViewFolder()
        {
            var viewFolder = new InMemoryViewFolder { { "bindings.xml", "<bindings><element name='foo'>bar</element></bindings>" } };
            var provider = new DefaultBindingProvider();
            var bindings = provider.GetBindings(viewFolder).ToList();

            Assert.That(bindings.Count, Is.EqualTo(1));
            Assert.That(bindings[0].ElementName, Is.EqualTo("foo"));
            Assert.That(bindings[0].Nodes.Count, Is.EqualTo(1));
            Assert.That(((BindingLiteral)bindings[0].Nodes[0]).Text, Is.EqualTo("bar"));
        }


        [Test]
        public void MissingFileDoesNotCauseException()
        {
            var viewFolder = new InMemoryViewFolder();
            var provider = new DefaultBindingProvider();
            var bindings = provider.GetBindings(viewFolder).ToList();

            Assert.That(bindings.Count, Is.EqualTo(0));
        }
    }
}
