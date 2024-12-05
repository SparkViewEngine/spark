using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Spark.Bindings;
using Spark.Parser;

namespace Spark.Tests.Bindings
{
    [TestFixture]
    public class BindingGrammarTester
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

            Assert.Multiple(() =>
            {
                Assert.That(result1.Value, Is.InstanceOf(typeof(BindingNameReference)));
                Assert.That(result2, Is.Null);
            });

            var value1 = (BindingNameReference)result1.Value;
            Assert.That(value1.Name, Is.EqualTo("caption"));

            var result3 = grammar.NameReference(Source("@extra stuff"));

            Assert.That(result3.Value, Is.InstanceOf(typeof(BindingNameReference)));
            var value3 = (BindingNameReference)result3.Value;
            Assert.That(value3.Name, Is.EqualTo("extra"));
        }


        [Test]
        public void AttrNameCanHaveSomeSpecialCharacters()
        {
            var grammar = new BindingGrammar();
            var result = grammar.NameReference(Source("@extra:special.more-name stuff"));

            Assert.That(result.Value, Is.InstanceOf(typeof(BindingNameReference)));
            var value = (BindingNameReference)result.Value;
            Assert.That(value.Name, Is.EqualTo("extra:special.more-name"));
        }

        [Test]
        public void MixedResultsComeBackAtTheRightPlaces()
        {
            var grammar = new BindingGrammar();
            var result = grammar.Nodes(Source("Html.ActionLink(@caption, @action)"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Count(), Is.EqualTo(5));

                Assert.That(((BindingLiteral)result.Value[0]).Text, Is.EqualTo("Html.ActionLink("));
                Assert.That(((BindingNameReference)result.Value[1]).Name, Is.EqualTo("caption"));
                Assert.That(((BindingLiteral)result.Value[2]).Text, Is.EqualTo(", "));
                Assert.That(((BindingNameReference)result.Value[3]).Name, Is.EqualTo("action"));
                Assert.That(((BindingLiteral)result.Value[4]).Text, Is.EqualTo(")"));
            });
        }

        [Test]
        public void PrefixRequiresTrailingAsterisk()
        {
            var grammar = new BindingGrammar();

            var result1 = grammar.PrefixReference(Source("@caption.*x"));
            var value1 = (BindingPrefixReference)result1.Value;
            Assert.Multiple(() =>
            {
                Assert.That(value1.Prefix, Is.EqualTo("caption."));
                Assert.That(result1.Rest.Peek(), Is.EqualTo('x'));
            });

            var result2 = grammar.PrefixReference(Source("@*y"));
            var value2 = (BindingPrefixReference)result2.Value;
            Assert.Multiple(() =>
            {
                Assert.That(value2.Prefix ?? "", Is.EqualTo(""));
                Assert.That(result2.Rest.Peek(), Is.EqualTo('y'));
            });
        }

        [Test]
        public void PrefixLetsYouWildcardAsTheLastCharacter()
        {
            var grammar = new BindingGrammar();
            var result = grammar.Nodes(Source("Html.ActionLink(@caption.**, @**)"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Count(), Is.EqualTo(5));

                Assert.That(((BindingLiteral)result.Value[0]).Text, Is.EqualTo("Html.ActionLink("));
                Assert.That(((BindingPrefixReference)result.Value[1]).Prefix, Is.EqualTo("caption."));
                Assert.That(((BindingLiteral)result.Value[2]).Text, Is.EqualTo("*, "));
                Assert.That(((BindingPrefixReference)result.Value[3]).Prefix ?? "", Is.EqualTo(""));
                Assert.That(((BindingLiteral)result.Value[4]).Text, Is.EqualTo("*)"));
            });
        }

        [Test]
        public void OptionalQuotesMarkAssumeStringValueAsTrue()
        {
            var grammar = new BindingGrammar();
            var result = grammar.Nodes(Source("@one'@two'\"@three\"@four.*'@five.*'\"@six.*\""));
            Assert.That(result.Value.Count(), Is.EqualTo(6));
            var one = (BindingNameReference)result.Value[0];
            var two = (BindingNameReference)result.Value[1];
            var three = (BindingNameReference)result.Value[2];
            var four = (BindingPrefixReference)result.Value[3];
            var five = (BindingPrefixReference)result.Value[4];
            var six = (BindingPrefixReference)result.Value[5];

            Assert.Multiple(() =>
            {
                Assert.That(one.AssumeStringValue, Is.False);
                Assert.That(two.AssumeStringValue, Is.True);
                Assert.That(three.AssumeStringValue, Is.True);
                Assert.That(four.AssumeStringValue, Is.False);
                Assert.That(five.AssumeStringValue, Is.True);
                Assert.That(six.AssumeStringValue, Is.True);
            });
        }

        [Test]
        public void ChildContentCanAppearWithOrWithoutQuotes()
        {
            var grammar = new BindingGrammar();
            var result = grammar.Nodes(Source("achild::*b'child::*'c\"child::*\"d"));
            Assert.That(result.Value.Count(), Is.EqualTo(7));
            var a = (BindingLiteral)result.Value[0];
            var child1 = (BindingChildReference)result.Value[1];
            var b = (BindingLiteral)result.Value[2];
            var child2 = (BindingChildReference)result.Value[3];
            var c = (BindingLiteral)result.Value[4];
            var child3 = (BindingChildReference)result.Value[5];
            var d = (BindingLiteral)result.Value[6];

            Assert.Multiple(() =>
            {
                Assert.That(a.Text, Is.EqualTo("a"));
                Assert.That(b.Text, Is.EqualTo("b"));
                Assert.That(c.Text, Is.EqualTo("c"));
                Assert.That(d.Text, Is.EqualTo("d"));
                Assert.That(child1, Is.Not.Null);
                Assert.That(child2, Is.Not.Null);
                Assert.That(child3, Is.Not.Null);
            });
        }

        [Test]
        public void CurleyBracesAroundWildcardIndicateDictionaryInitializingSyntax()
        {
            var grammar = new BindingGrammar();
            var result = grammar.Nodes(Source("a{{@*}}{{@hello*}}b"));

            Assert.That(result.Value.Count(), Is.EqualTo(4));
            var a = (BindingLiteral)result.Value[0];
            var match1 = (BindingPrefixReference)result.Value[1];
            var match2 = (BindingPrefixReference)result.Value[2];
            var b = (BindingLiteral)result.Value[3];

            Assert.Multiple(() =>
            {
                Assert.That(a.Text, Is.EqualTo("a"));
                Assert.That(b.Text, Is.EqualTo("b"));
                Assert.That(match1.Prefix, Is.Null);
                Assert.That(match2.Prefix, Is.EqualTo("hello"));
                Assert.That(match1.AssumeDictionarySyntax, Is.True);
                Assert.That(match2.AssumeDictionarySyntax, Is.True);
                Assert.That(match1.AssumeStringValue, Is.False);
                Assert.That(match2.AssumeStringValue, Is.False);
            });
        }

        [Test]
        public void BothBracesMustBePresentToMatch()
        {
            var grammar = new BindingGrammar();
            var result = grammar.Nodes(Source("a{{@*@hello*}}b"));

            Assert.That(result.Value.Count(), Is.EqualTo(4));
            var a = (BindingLiteral)result.Value[0];
            var match1 = (BindingPrefixReference)result.Value[1];
            var match2 = (BindingPrefixReference)result.Value[2];
            var b = (BindingLiteral)result.Value[3];

            Assert.Multiple(() =>
            {
                Assert.That(a.Text, Is.EqualTo("a{{"));
                Assert.That(b.Text, Is.EqualTo("}}b"));
                Assert.That(match1.Prefix, Is.Null);
                Assert.That(match2.Prefix, Is.EqualTo("hello"));
                Assert.That(match1.AssumeDictionarySyntax, Is.False);
                Assert.That(match2.AssumeDictionarySyntax, Is.False);
            });
        }

        [Test]
        public void BracesMayAppearAroundStrings()
        {
            var grammar = new BindingGrammar();
            var result = grammar.Nodes(Source("a{{'@*'}}{{\"@hello*\"}}b"));

            Assert.That(result.Value.Count(), Is.EqualTo(4));
            var a = (BindingLiteral)result.Value[0];
            var match1 = (BindingPrefixReference)result.Value[1];
            var match2 = (BindingPrefixReference)result.Value[2];
            var b = (BindingLiteral)result.Value[3];

            Assert.Multiple(() =>
            {
                Assert.That(a.Text, Is.EqualTo("a"));
                Assert.That(b.Text, Is.EqualTo("b"));
                Assert.That(match1.Prefix, Is.Null);
                Assert.That(match2.Prefix, Is.EqualTo("hello"));
                Assert.That(match1.AssumeDictionarySyntax, Is.True);
                Assert.That(match2.AssumeDictionarySyntax, Is.True);
                Assert.That(match1.AssumeStringValue, Is.True);
                Assert.That(match2.AssumeStringValue, Is.True);
            });
        }

        [Test]
        public void DoubleSquareBracketIsAliasForAngleBracket()
        {
            var grammar = new BindingGrammar();
            var result = grammar.Nodes(Source("this[[that]]"));

            Assert.That(result.Value.Count(), Is.EqualTo(1));
            var node = (BindingLiteral)result.Value[0];

            Assert.That(node.Text, Is.EqualTo("this<that>"));
        }
    }
}
