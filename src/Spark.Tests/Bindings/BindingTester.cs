using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.Bindings;
using Spark.Parser;

namespace Spark.Tests.Bindings
{
    [TestFixture]
    public class BindingTester
    {
        private Position Source(string code)
        {
            return new Position(new SourceContext(code));
        }

        [Test]
        public void BindingGrammarCanReadAnEntireString()
        {
            var grammar = new BindingGrammar();
            var pos = new Position(new SourceContext("hello world"));
            var result = (BindingLiteral)grammar.Literal(pos).Value;
            Assert.That(result.Text, Is.EqualTo("hello world"));
        }

        [Test]
        public void AttrsRecognizedStartingWithAtAndEndAfterName()
        {
            var grammar = new BindingGrammar();
            var result1 = grammar.NameReference(Source("@caption"));
            var result2 = grammar.NameReference(Source(" @caption"));

            Assert.That(result1.Value, Is.InstanceOfType(typeof(BindingNameReference)));
            Assert.That(result2, Is.Null);

            var value1 = (BindingNameReference)result1.Value;
            Assert.That(value1.Name, Is.EqualTo("caption"));

            var result3 = grammar.NameReference(Source("@extra stuff"));

            Assert.That(result3.Value, Is.InstanceOfType(typeof(BindingNameReference)));
            var value3 = (BindingNameReference)result3.Value;
            Assert.That(value3.Name, Is.EqualTo("extra"));
        }


        [Test]
        public void AttrNameCanHaveSomeSpecialCharacters()
        {
            var grammar = new BindingGrammar();
            var result = grammar.NameReference(Source("@extra:special.more-name stuff"));

            Assert.That(result.Value, Is.InstanceOfType(typeof(BindingNameReference)));
            var value = (BindingNameReference)result.Value;
            Assert.That(value.Name, Is.EqualTo("extra:special.more-name"));
        }

        [Test]
        public void MixedResultsComeBackAtTheRightPlaces()
        {
            var grammar = new BindingGrammar();
            var result = grammar.Nodes(Source("Html.ActionLink(@caption, @action)"));

            Assert.That(result.Value.Count(), Is.EqualTo(5));

            Assert.That(((BindingLiteral)result.Value[0]).Text, Is.EqualTo("Html.ActionLink("));
            Assert.That(((BindingNameReference)result.Value[1]).Name, Is.EqualTo("caption"));
            Assert.That(((BindingLiteral)result.Value[2]).Text, Is.EqualTo(", "));
            Assert.That(((BindingNameReference)result.Value[3]).Name, Is.EqualTo("action"));
            Assert.That(((BindingLiteral)result.Value[4]).Text, Is.EqualTo(")"));
        }

        [Test]
        public void PrefixRequiresTrailingAsterisk()
        {
            var grammar = new BindingGrammar();

            var result1 = grammar.PrefixReference(Source("@caption.*x"));
            var value1 = (BindingPrefixReference)result1.Value;
            Assert.That(value1.Prefix, Is.EqualTo("caption."));
            Assert.That(result1.Rest.Peek(), Is.EqualTo('x'));

            var result2 = grammar.PrefixReference(Source("@*y"));
            var value2 = (BindingPrefixReference)result2.Value;
            Assert.That(value2.Prefix ?? "", Is.EqualTo(""));
            Assert.That(result2.Rest.Peek(), Is.EqualTo('y'));
        }

        [Test]
        public void PrefixLetsYouWildcardAsTheLastCharacter()
        {
            var grammar = new BindingGrammar();
            var result = grammar.Nodes(Source("Html.ActionLink(@caption.**, @**)"));

            Assert.That(result.Value.Count(), Is.EqualTo(5));

            Assert.That(((BindingLiteral)result.Value[0]).Text, Is.EqualTo("Html.ActionLink("));
            Assert.That(((BindingPrefixReference)result.Value[1]).Prefix, Is.EqualTo("caption."));
            Assert.That(((BindingLiteral)result.Value[2]).Text, Is.EqualTo("*, "));
            Assert.That(((BindingPrefixReference)result.Value[3]).Prefix ?? "", Is.EqualTo(""));
            Assert.That(((BindingLiteral)result.Value[4]).Text, Is.EqualTo("*)"));
        }
    }
}
