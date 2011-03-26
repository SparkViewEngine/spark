using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Spark.Compiler.Javascript.ChunkVisitors;
using Spark.Parser;

namespace Spark.Tests.Parser
{
    [TestFixture]
    public class JavascriptAnonymousTypeGrammarTester
    {
        private JavascriptAnonymousTypeGrammar _grammar;

        [SetUp]
        public void SetUp()
        {
            _grammar = new JavascriptAnonymousTypeGrammar();
        }

        static Position Source(string content)
        {
            return new Position(new SourceContext(content));
        }

        static string Parse(ParseAction<string> parser, string content)
        {
            var result = parser(Source(content));
            return result != null ? result.Value : null;
        }

        [Test]
        public void WsShouldMatchWhitespace()
        {
            var result = _grammar.test_ws(Source("  \r\n  \t hello world"));
            Assert.That(result.Value, Is.EqualTo("  \r\n  \t "));
        }

        [Test]
        public void PropertyNameIsAlphaOrScoreFollowedByAlnumscore()
        {
            var p = _grammar.test_propName;
            Assert.That(Parse(p, "hello"), Is.EqualTo("hello"));
            Assert.That(Parse(p, "h_ello4-6"), Is.EqualTo("h_ello4"));
            Assert.That(Parse(p, "_foo2+4"), Is.EqualTo("_foo2"));
            Assert.That(Parse(p, "45ee-2"), Is.Null);
        }

        [Test]
        public void PropertyValueStopsAtUnprotectedCloseBraceOrComma()
        {
            var p = _grammar.test_propValue;
            Assert.That(Parse(p, " 45 - 3;}"), Is.EqualTo(" 45 - 3;"));
            Assert.That(Parse(p, "x'},'x}"), Is.EqualTo("x'},'x"));
            Assert.That(Parse(p, "x\"},\"x}"), Is.EqualTo("x\"},\"x"));
            Assert.That(Parse(p, "x'},'x,"), Is.EqualTo("x'},'x"));
            Assert.That(Parse(p, "x\"},\"x,"), Is.EqualTo("x\"},\"x"));
        }

        [Test]
        public void DoubleAndSingleStringStopAtCorrectCharacter()
        {
            var p = _grammar.test_doubleString;
            Assert.That(Parse(p, "\"hello world\" 2"), Is.EqualTo("\"hello world\""));
            Assert.That(Parse(p, " \"x\" "), Is.Null);

            p = _grammar.test_singleString;
            Assert.That(Parse(p, "'foo'-"), Is.EqualTo("'foo'"));
            Assert.That(Parse(p, " 'x' "), Is.Null);
        }

        [Test]
        public void TermShouldFindNameDelimiterValueAndWhitespace()
        {
            var p = _grammar.test_term;
            Assert.That(Parse(p, "foo='bar'"), Is.EqualTo("foo='bar'"));
            Assert.That(Parse(p, " foo = 'bar' "), Is.EqualTo(" foo = 'bar' "));
            Assert.That(Parse(p, " foo  'bar' "), Is.Null);
            Assert.That(Parse(p, "  = 'bar' "), Is.Null);
            Assert.That(Parse(p, " 3 = 5 "), Is.Null);
        }

        [Test]
        public void TermValueShouldStopAtCloseBraceOrComma()
        {
            var p = _grammar.test_term;
            Assert.That(Parse(p, "foo='bar'}"), Is.EqualTo("foo='bar'"));
            Assert.That(Parse(p, "foo='bar',"), Is.EqualTo("foo='bar'"));
            Assert.That(Parse(p, " foo = 'bar' }"), Is.EqualTo(" foo = 'bar' "));
            Assert.That(Parse(p, " foo = 'bar' ,"), Is.EqualTo(" foo = 'bar' "));
        }

        [Test]
        public void SeveralTermsMayBeSeperatedByComma()
        {
            var p = _grammar.test_terms;
            Assert.That(Parse(p, "foo='bar'}"), Is.EqualTo("foo='bar'"));
            Assert.That(Parse(p, "foo='bar',"), Is.EqualTo("foo='bar'"));
            Assert.That(Parse(p, " foo = 'bar' }"), Is.EqualTo(" foo = 'bar' "));
            Assert.That(Parse(p, " foo = 'bar' ,"), Is.EqualTo(" foo = 'bar' "));

            Assert.That(Parse(p, "foo='bar',quad=5}"), Is.EqualTo("foo='bar',quad=5"));
            Assert.That(Parse(p, "foo='bar',quad=5,"), Is.EqualTo("foo='bar',quad=5"));
            Assert.That(Parse(p, " foo = 'bar' , quad = 5 }"), Is.EqualTo(" foo = 'bar' , quad = 5 "));
            Assert.That(Parse(p, " foo = 'bar' , quad = 5 ,"), Is.EqualTo(" foo = 'bar' , quad = 5 "));
        }

        [Test]
        public void AnonymousTypeShouldUnderstandAndConvertSyntax()
        {
            var p = _grammar.test_anonymousType;
            Assert.That(Parse(p, "new {x = 4} "), Is.EqualTo("{x:4}"));
            Assert.That(Parse(p, "new {x = 4, y = 3} "), Is.EqualTo("{x:4,y:3}"));
            Assert.That(Parse(p, "new {x = ',}', y = 3} "), Is.EqualTo("{x:',}',y:3}"));
        }

        [Test]
        public void ReformatCodeShouldConvertAnonymousTypesRecursively()
        {
            var p = _grammar.ReformatCode;

            Assert.That(Parse(p, "hello world"), Is.EqualTo("hello world"));
            Assert.That(Parse(p, "Html.ActionLink('a','b',new {c='d', e='f'})"), Is.EqualTo("Html.ActionLink('a','b',{c:'d',e:'f'})"));
            Assert.That(Parse(p, "Html.ActionLink('a','b',new {c='d', e=new{f=3, g='h'}})"), Is.EqualTo("Html.ActionLink('a','b',{c:'d',e:{f:3,g:'h'}})"));
        }
    }
}
