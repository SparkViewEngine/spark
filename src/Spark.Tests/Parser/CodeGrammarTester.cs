// Copyright 2008 Louis DeJardin - http://whereslou.com
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
            Assert.AreEqual("hello world", Combine(result.Value));
        }


        [Test]
        public void StringConstants()
        {
            var result = _grammar.Expression(Source("double\"quote\"strings"));
            Assert.AreEqual("double\"quote\"strings", Combine(result.Value));

            var result2 = _grammar.Expression(Source("single\'quote\'strings"));
            Assert.AreEqual("single\"quote\"strings", Combine(result2.Value));
        }

        [Test]
        public void EnclosedEscapes()
        {
            var result = _grammar.Expression(Source("double\"quote-'-\\\'-\\\"-\"strings"));
            Assert.AreEqual("double\"quote-'-\\\'-\\\"-\"strings", Combine(result.Value));

            var result2 = _grammar.Expression(Source("single\'quote-\"-\\\'-\\\"-\'strings"));
            Assert.AreEqual("single\"quote-\\\"-\\\'-\\\"-\"strings", Combine(result2.Value));
        }

        [Test]
        public void BraceMatching()
        {
            var result = _grammar.Expression(Source("Html.Link(new {x='ten', y=20}) a}b "));
            Assert.AreEqual("Html.Link(new {x=\"ten\", y=20}) a", Combine(result.Value));
        }

        [Test]
        public void StopAtExpressionTerminators()
        {
            var result = _grammar.Expression(Source("ab{cde{fgh}ijk}lm\"n}o\"p'}}%>'qrs}tuv"));
            Assert.AreEqual("ab{cde{fgh}ijk}lm\"n}o\"p\"}}%>\"qrs", Combine(result.Value));

            var result2 = _grammar.Expression(Source("ab{cde{fgh}ijk}lm\"n}o\"p'}}%>'qrs%>tuv"));
            Assert.AreEqual("ab{cde{fgh}ijk}lm\"n}o\"p\"}}%>\"qrs", Combine(result2.Value));
            
        }

        [Test]
        public void SpecialCastAllowsCharConstant()
        {
            var result = _grammar.Expression(Source("..(char)'u'..'u'.."));
            Assert.AreEqual("..(char)'u'..\"u\"..", Combine(result.Value));
            
        }


        [Test]
        public void ChangeDoubleBraceAliases()
        {
            var result = _grammar.Expression(Source("one < two > three [[ four '[[fi\"ve]]' ]] six \"[[']]\" seven"));
            Assert.AreEqual("one < two > three < four \"[[fi\\\"ve]]\" > six \"[[']]\" seven", Combine(result.Value));

        }

        [Test]
        public void Statement1StopsWithEndOfLine()
        {
            var result = _grammar.Statement1(Source("before%>and\r\nafter"));
            Assert.AreEqual("before%>and", Combine(result.Value));
        }

        [Test]
        public void Statement2StopsWithPercentAngle()
        {
            var result = _grammar.Statement2(Source("before\r\nand%>after"));
            Assert.AreEqual("before\r\nand", Combine(result.Value));
        }


        [Test]
        public void StringsMayHavePercentAngle()
        {
            var result = _grammar.Statement1(Source("before\"%>\"and'%>'after\r\nagain"));
            Assert.AreEqual("before\"%>\"and\"%>\"after", Combine(result.Value));
            var result2 = _grammar.Statement2(Source("before\"%>\"and'%>'after%>again"));
            Assert.AreEqual("before\"%>\"and\"%>\"after", Combine(result2.Value));
        }

        [Test]
        public void StatementMayContainUnmatchedBraces()
        {
            var result = _grammar.Statement1(Source("this{that"));
            Assert.AreEqual("this{that", Combine(result.Value));
            var result2 = _grammar.Statement1(Source("this}that"));
            Assert.AreEqual("this}that", Combine(result2.Value));
            var result3 = _grammar.Statement2(Source("this{that"));
            Assert.AreEqual("this{that", Combine(result3.Value));
            var result4 = _grammar.Statement2(Source("this}that"));
            Assert.AreEqual("this}that", Combine(result4.Value));
        }

        [Test]
        public void VerbatimDoubleQuotes()
        {
            var result = _grammar.Expression(Source("a@\" \\\"\" \"b"));
            Assert.AreEqual("a@\" \\\"\" \"b", Combine(result.Value));

            var result2 = _grammar.Expression(Source("a@\" \\\"\"} \"b"));
            Assert.AreEqual("a@\" \\\"\"} \"b", Combine(result2.Value));
        }

        [Test]
        public void VerbatimSingleQuotes()
        {
            //@' \'' ' becomes @" \' "
            var result = _grammar.Expression(Source("a@' \\'' 'b"));
            Assert.AreEqual("a@\" \\' \"b", Combine(result.Value));

            //@' \''} ' becomes @" \'} "
            var result2 = _grammar.Expression(Source("a@' \\''} 'b"));
            Assert.AreEqual("a@\" \\'} \"b", Combine(result2.Value));

            //@' " '' ' becomes @" "" ' "
            var result3 = _grammar.Expression(Source("a@' \" '' 'b"));
            Assert.AreEqual("a@\" \"\" ' \"b", Combine(result3.Value));
        }

        [Test]
        public void CommentHasQuotes()
        {
            var result = _grammar.Statement1(Source(" // this ' has \" quotes \r\n after "));
            Assert.AreEqual(" // this ' has \" quotes ", Combine(result.Value));

            var result2 = _grammar.Statement1(Source(" /* this ' has \" quotes \r\n */ more \r\n after "));
            Assert.AreEqual(" /* this ' has \" quotes \r\n */ more ", Combine(result2.Value));

            var result3 = _grammar.Statement2(Source(" // this ' has \" quotes \r\n more %> after "));
            Assert.AreEqual(" // this ' has \" quotes \r\n more ", Combine(result3.Value));

            var result4 = _grammar.Statement2(Source(" /* this ' has \" quotes \r\n */ more %> after "));
            Assert.AreEqual(" /* this ' has \" quotes \r\n */ more ", Combine(result4.Value));
        }

        [Test]
        public void ClassKeywordUsedAsIdentifier()
        {
            var result = _grammar.Expression(Source("Form.FormTag(new {action='foo', class='bar'})"));
            Assert.AreEqual(@"Form.FormTag(new {action=""foo"", @class=""bar""})", Combine(result.Value));

            var result2 = _grammar.Expression(Source("Form.FormTag(new {action='foo', @class='bar'})"));
            Assert.AreEqual(@"Form.FormTag(new {action=""foo"", @class=""bar""})", Combine(result2.Value));

            var result3 = _grammar.Expression(Source("Form.FormTag(new {@action='foo', class='bar'})"));
            Assert.AreEqual(@"Form.FormTag(new {@action=""foo"", @class=""bar""})", Combine(result3.Value));

            var result4 = _grammar.Expression(Source("var classless=1;"));
            Assert.AreEqual(@"var classless=1;", Combine(result4.Value));

            var result5 = _grammar.Expression(Source("var yaddaclass=1;"));
            Assert.AreEqual(@"var yaddaclass=1;", Combine(result5.Value));

            var result6 = _grammar.Expression(Source("var declassified=1;"));
            Assert.AreEqual(@"var declassified=1;", Combine(result6.Value));

            var result7 = _grammar.Expression(Source("var class=1;"));
            Assert.AreEqual(@"var @class=1;", Combine(result7.Value));
        }

        [Test]        
        public void LateBoundSyntaxBecomesEvalFunction()
        {
            var result1 = _grammar.Expression(Source("#foo.bar"));
            Assert.AreEqual(@"Eval(""foo.bar"")", (string) result1.Value);

            var result2 = _grammar.Expression(Source("#foo .bar"));
            Assert.AreEqual(@"Eval(""foo"") .bar", (string)result2.Value);

            var result3 = _grammar.Expression(Source("(string)#foo+'bar'"));
            Assert.AreEqual(@"(string)Eval(""foo"")+""bar""", (string)result3.Value);

            var result4 = _grammar.Statement1(Source("Logger.Warn(#some.thing)"));
            Assert.AreEqual(@"Logger.Warn(Eval(""some.thing""))", (string)new Snippets(result4.Value));

            var result5 = _grammar.Statement1(Source("Logger.Warn(#some.thing)"));
            Assert.AreEqual(@"Logger.Warn(Eval(""some.thing""))", (string)new Snippets(result5.Value));
        }

    }
}