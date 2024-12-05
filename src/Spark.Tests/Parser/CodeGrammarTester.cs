// Copyright 2008-2024 Louis DeJardin
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System.Collections.Generic;
using NUnit.Framework;
using Spark.Parser;
using Spark.Parser.Code;
using System.Linq;

namespace Spark.Tests.Parser
{
    [TestFixture]
    public class CodeGrammarTester
    {
        private CodeGrammar _grammar;

        [SetUp]
        public void Init()
        {
            _grammar = new CodeGrammar();
        }

        static Position Source(string text)
        {
            return new Position(new SourceContext(text));
        }

        static string Combine(IList<Snippet> snippets)
        {
            return string.Concat(snippets.Select(s => s.Value).ToArray());
        }

        [Test]
        public void SimpleStatement()
        {
            var result = _grammar.Expression(Source("hello world"));
            Assert.That(Combine(result.Value), Is.EqualTo("hello world"));
        }


        [Test]
        public void StringConstants()
        {
            var result = _grammar.Expression(Source("double\"quote\"strings"));
            Assert.That(Combine(result.Value), Is.EqualTo("double\"quote\"strings"));

            var result2 = _grammar.Expression(Source("single\'quote\'strings"));
            Assert.That(Combine(result2.Value), Is.EqualTo("single\"quote\"strings"));
        }

        [Test]
        public void EnclosedEscapes()
        {
            var result = _grammar.Expression(Source("double\"quote-'-\\\'-\\\"-\"strings"));
            Assert.That(Combine(result.Value), Is.EqualTo("double\"quote-'-\\\'-\\\"-\"strings"));

            var result2 = _grammar.Expression(Source("single\'quote-\"-\\\'-\\\"-\'strings"));
            Assert.That(Combine(result2.Value), Is.EqualTo("single\"quote-\\\"-\\\'-\\\"-\"strings"));
        }

        [Test]
        public void BraceMatching()
        {
            var result = _grammar.Expression(Source("Html.Link(new {x='ten', y=20}) a}b "));
            Assert.That(Combine(result.Value), Is.EqualTo("Html.Link(new {x=\"ten\", y=20}) a"));
        }

        [Test]
        public void StopAtExpressionTerminators()
        {
            var result = _grammar.Expression(Source("ab{cde{fgh}ijk}lm\"n}o\"p'}}%>'qrs}tuv"));
            Assert.That(Combine(result.Value), Is.EqualTo("ab{cde{fgh}ijk}lm\"n}o\"p\"}}%>\"qrs"));

            var result2 = _grammar.Expression(Source("ab{cde{fgh}ijk}lm\"n}o\"p'}}%>'qrs%>tuv"));
            Assert.That(Combine(result2.Value), Is.EqualTo("ab{cde{fgh}ijk}lm\"n}o\"p\"}}%>\"qrs"));

        }

        [Test]
        public void SpecialCastAllowsCharConstant()
        {
            var result = _grammar.Expression(Source("..(char)'u'..'u'.."));
            Assert.That(Combine(result.Value), Is.EqualTo("..(char)'u'..\"u\".."));

        }


        [Test]
        public void ChangeDoubleBraceAliases()
        {
            var result = _grammar.Expression(Source("one < two > three [[ four '[[fi\"ve]]' ]] six \"[[']]\" seven"));
            Assert.That(Combine(result.Value), Is.EqualTo("one < two > three < four \"[[fi\\\"ve]]\" > six \"[[']]\" seven"));

        }

        [Test]
        public void Statement1StopsWithEndOfLine()
        {
            var result = _grammar.Statement1(Source("before%>and\r\nafter"));
            Assert.That(Combine(result.Value), Is.EqualTo("before%>and"));
        }

        [Test]
        public void Statement2StopsWithPercentAngle()
        {
            var result = _grammar.Statement2(Source("before\r\nand%>after"));
            Assert.That(Combine(result.Value), Is.EqualTo("before\r\nand"));
        }


        [Test]
        public void StringsMayHavePercentAngle()
        {
            var result = _grammar.Statement1(Source("before\"%>\"and'%>'after\r\nagain"));
            Assert.That(Combine(result.Value), Is.EqualTo("before\"%>\"and\"%>\"after"));
            var result2 = _grammar.Statement2(Source("before\"%>\"and'%>'after%>again"));
            Assert.That(Combine(result2.Value), Is.EqualTo("before\"%>\"and\"%>\"after"));
        }

        [Test]
        public void StatementMayContainUnmatchedBraces()
        {
            var result = _grammar.Statement1(Source("this{that"));
            Assert.That(Combine(result.Value), Is.EqualTo("this{that"));
            var result2 = _grammar.Statement1(Source("this}that"));
            Assert.That(Combine(result2.Value), Is.EqualTo("this}that"));
            var result3 = _grammar.Statement2(Source("this{that"));
            Assert.That(Combine(result3.Value), Is.EqualTo("this{that"));
            var result4 = _grammar.Statement2(Source("this}that"));
            Assert.That(Combine(result4.Value), Is.EqualTo("this}that"));
        }

        [Test]
        public void VerbatimDoubleQuotes()
        {
            var result = _grammar.Expression(Source("a@\" \\\"\" \"b"));
            Assert.That(Combine(result.Value), Is.EqualTo("a@\" \\\"\" \"b"));

            var result2 = _grammar.Expression(Source("a@\" \\\"\"} \"b"));
            Assert.That(Combine(result2.Value), Is.EqualTo("a@\" \\\"\"} \"b"));
        }

        [Test]
        public void VerbatimSingleQuotes()
        {
            //@' \'' ' becomes @" \' "
            var result = _grammar.Expression(Source("a@' \\'' 'b"));
            Assert.That(Combine(result.Value), Is.EqualTo("a@\" \\' \"b"));

            //@' \''} ' becomes @" \'} "
            var result2 = _grammar.Expression(Source("a@' \\''} 'b"));
            Assert.That(Combine(result2.Value), Is.EqualTo("a@\" \\'} \"b"));

            //@' " '' ' becomes @" "" ' "
            var result3 = _grammar.Expression(Source("a@' \" '' 'b"));
            Assert.That(Combine(result3.Value), Is.EqualTo("a@\" \"\" ' \"b"));
        }

        [Test]
        public void CommentHasQuotes()
        {
            var result = _grammar.Statement1(Source(" // this ' has \" quotes \r\n after "));
            Assert.That(Combine(result.Value), Is.EqualTo(" // this ' has \" quotes "));

            var result2 = _grammar.Statement1(Source(" /* this ' has \" quotes \r\n */ more \r\n after "));
            Assert.That(Combine(result2.Value), Is.EqualTo(" /* this ' has \" quotes \r\n */ more "));

            var result3 = _grammar.Statement2(Source(" // this ' has \" quotes \r\n more %> after "));
            Assert.That(Combine(result3.Value), Is.EqualTo(" // this ' has \" quotes \r\n more "));

            var result4 = _grammar.Statement2(Source(" /* this ' has \" quotes \r\n */ more %> after "));
            Assert.That(Combine(result4.Value), Is.EqualTo(" /* this ' has \" quotes \r\n */ more "));
        }

        [Test]
        public void ClassKeywordUsedAsIdentifier()
        {
            var result = _grammar.Expression(Source("Form.FormTag(new {action='foo', class='bar'})"));
            Assert.That(Combine(result.Value), Is.EqualTo(@"Form.FormTag(new {action=""foo"", @class=""bar""})"));

            var result2 = _grammar.Expression(Source("Form.FormTag(new {action='foo', @class='bar'})"));
            Assert.That(Combine(result2.Value), Is.EqualTo(@"Form.FormTag(new {action=""foo"", @class=""bar""})"));

            var result3 = _grammar.Expression(Source("Form.FormTag(new {@action='foo', class='bar'})"));
            Assert.That(Combine(result3.Value), Is.EqualTo(@"Form.FormTag(new {@action=""foo"", @class=""bar""})"));

            var result4 = _grammar.Expression(Source("var classless=1;"));
            Assert.That(Combine(result4.Value), Is.EqualTo(@"var classless=1;"));

            var result5 = _grammar.Expression(Source("var yaddaclass=1;"));
            Assert.That(Combine(result5.Value), Is.EqualTo(@"var yaddaclass=1;"));

            var result6 = _grammar.Expression(Source("var declassified=1;"));
            Assert.That(Combine(result6.Value), Is.EqualTo(@"var declassified=1;"));

            var result7 = _grammar.Expression(Source("var class=1;"));
            Assert.That(Combine(result7.Value), Is.EqualTo(@"var @class=1;"));
        }

        [Test]
        public void LateBoundSyntaxBecomesEvalFunction()
        {
            var result1 = _grammar.Expression(Source("#foo.bar"));
            Assert.That((string)result1.Value, Is.EqualTo(@"Eval(""foo.bar"")"));

            var result2 = _grammar.Expression(Source("#foo .bar"));
            Assert.That((string)result2.Value, Is.EqualTo(@"Eval(""foo"") .bar"));

            var result3 = _grammar.Expression(Source("(string)#foo+'bar'"));
            Assert.That((string)result3.Value, Is.EqualTo(@"(string)Eval(""foo"")+""bar"""));

            var result4 = _grammar.Statement1(Source("Logger.Warn(#some.thing)"));
            Assert.That((string)new Snippets(result4.Value), Is.EqualTo(@"Logger.Warn(Eval(""some.thing""))"));

            var result5 = _grammar.Statement1(Source("Logger.Warn(#some.thing)"));
            Assert.That((string)new Snippets(result5.Value), Is.EqualTo(@"Logger.Warn(Eval(""some.thing""))"));
        }

    }
}