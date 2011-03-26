using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Rhino.Mocks;
using Spark.Bindings;
using Spark.Compiler;
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
            Assert.That(bindings[0].Phrases.Single().Nodes.Count, Is.EqualTo(1));
            Assert.That(((BindingLiteral)bindings[0].Phrases.Single().Nodes[0]).Text, Is.EqualTo("bar"));
            Assert.That(bindings[0].Phrases.All(phrase => phrase.Type == BindingPhrase.PhraseType.Expression));
        }


        [Test]
        public void MissingFileDoesNotCauseException()
        {
            var viewFolder = new InMemoryViewFolder();
            var provider = new DefaultBindingProvider();
            var bindings = provider.GetBindings(viewFolder).ToList();

            Assert.That(bindings.Count, Is.EqualTo(0));
        }

        [Test]
        public void TwoPartBindingsAreRecognized()
        {
            var viewFolder = new InMemoryViewFolder { { "bindings.xml", "<bindings><element name='foo'><start>bar</start><end>quux</end></element></bindings>" } };
            var provider = new DefaultBindingProvider();
            var bindings = provider.GetBindings(viewFolder).ToList();

            Assert.That(bindings.Count, Is.EqualTo(1));
            Assert.That(bindings[0].ElementName, Is.EqualTo("foo"));
            Assert.That(bindings[0].Phrases.Count(), Is.EqualTo(2));
            Assert.That(bindings[0].Phrases.First().Nodes.Count, Is.EqualTo(1));
            Assert.That(bindings[0].Phrases.Last().Nodes.Count, Is.EqualTo(1));
            Assert.That(((BindingLiteral)bindings[0].Phrases.First().Nodes[0]).Text, Is.EqualTo("bar"));
            Assert.That(((BindingLiteral)bindings[0].Phrases.Last().Nodes[0]).Text, Is.EqualTo("quux"));
            Assert.That(bindings[0].Phrases.All(phrase => phrase.Type == BindingPhrase.PhraseType.Expression));
        }

        [Test]
        public void HashCanBeUsedToDeclareStatementsInsteadOfOutputExpressions()
        {
            var viewFolder = new InMemoryViewFolder { { "bindings.xml", "<bindings><element name='foo'><start>#bar;</start><end>#quux;</end></element></bindings>" } };
            var provider = new DefaultBindingProvider();
            var bindings = provider.GetBindings(viewFolder).ToList();
            Assert.That(bindings[0].Phrases.All(phrase => phrase.Type == BindingPhrase.PhraseType.Statement));
        }


        [Test]
        public void ChildReferenceMayNotAppearInStartPhrase()
        {
            var viewFolder = new InMemoryViewFolder { { "bindings.xml", "<bindings><element name='foo'><start>child::*</start><end>foo</end></element></bindings>" } };
            var provider = new DefaultBindingProvider();
            Assert.That(() => provider.GetBindings(viewFolder).ToList(), Throws.TypeOf<CompilerException>());
        }

        [Test]
        public void ChildReferenceMayNotAppearInEndPhrase()
        {
            var viewFolder = new InMemoryViewFolder { { "bindings.xml", "<bindings><element name='foo'><start>foo</start><end>child::*</end></element></bindings>" } };
            var provider = new DefaultBindingProvider();
            Assert.That(() => provider.GetBindings(viewFolder).ToList(), Throws.TypeOf<CompilerException>());
        }
    }
}
