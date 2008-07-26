/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using NUnit.Framework;
using Spark.Parser;
using Spark.Parser.Code;

namespace Spark.Tests
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

        Position Source(string text)
        {
            return new Position(new SourceContext(text));
        }

        [Test]
        public void SimpleStatement()
        {
            var result = _grammar.Expression(Source("hello world"));
            Assert.AreEqual("hello world", result.Value);
        }


        [Test]
        public void StringConstants()
        {
            var result = _grammar.Expression(Source("double\"quote\"strings"));
            Assert.AreEqual("double\"quote\"strings", result.Value);

            var result2 = _grammar.Expression(Source("single\'quote\'strings"));
            Assert.AreEqual("single\"quote\"strings", result2.Value);
        }

        [Test]
        public void EnclosedEscapes()
        {
            var result = _grammar.Expression(Source("double\"quote-'-\\\'-\\\"-\"strings"));
            Assert.AreEqual("double\"quote-'-\\\'-\\\"-\"strings", result.Value);

            var result2 = _grammar.Expression(Source("single\'quote-\"-\\\'-\\\"-\'strings"));
            Assert.AreEqual("single\"quote-\\\"-\\\'-\\\"-\"strings", result2.Value);
        }

        [Test]
        public void BraceMatching()
        {
            var result = _grammar.Expression(Source("Html.Link(new {x='ten', y=20}) a}b "));
            Assert.AreEqual("Html.Link(new {x=\"ten\", y=20}) a", result.Value);
        }

        [Test]
        public void StopAtExpressionTerminators()
        {
            var result = _grammar.Expression(Source("ab{cde{fgh}ijk}lm\"n}o\"p'}}%>'qrs}tuv"));
            Assert.AreEqual("ab{cde{fgh}ijk}lm\"n}o\"p\"}}%>\"qrs", result.Value);

            var result2 = _grammar.Expression(Source("ab{cde{fgh}ijk}lm\"n}o\"p'}}%>'qrs%>tuv"));
            Assert.AreEqual("ab{cde{fgh}ijk}lm\"n}o\"p\"}}%>\"qrs", result2.Value);
            
        }

        [Test]
        public void SpecialCastAllowsCharConstant()
        {
            var result = _grammar.Expression(Source("..(char)'u'..'u'.."));
            Assert.AreEqual("..(char)'u'..\"u\"..", result.Value);
            
        }


        [Test]
        public void ChangeDoubleBraceAliases()
        {
            var result = _grammar.Expression(Source("one < two > three [[ four '[[fi\"ve]]' ]] six \"[[']]\" seven"));
            Assert.AreEqual("one < two > three < four \"[[fi\\\"ve]]\" > six \"[[']]\" seven", result.Value);

        }

        [Test]
        public void Statement1StopsWithEndOfLine()
        {
            var result = _grammar.Statement1(Source("before%>and\r\nafter"));
            Assert.AreEqual("before%>and", result.Value);
        }

        [Test]
        public void Statement2StopsWithPercentAngle()
        {
            var result = _grammar.Statement2(Source("before\r\nand%>after"));
            Assert.AreEqual("before\r\nand", result.Value);
        }


        [Test]
        public void StringsMayHavePercentAngle()
        {
            var result = _grammar.Statement1(Source("before\"%>\"and'%>'after\r\nagain"));
            Assert.AreEqual("before\"%>\"and\"%>\"after", result.Value);
            var result2 = _grammar.Statement2(Source("before\"%>\"and'%>'after%>again"));
            Assert.AreEqual("before\"%>\"and\"%>\"after", result2.Value);
        }

        [Test]
        public void StatementMayContainUnmatchedBraces()
        {
            var result = _grammar.Statement1(Source("this{that"));
            Assert.AreEqual("this{that", result.Value);
            var result2 = _grammar.Statement1(Source("this}that"));
            Assert.AreEqual("this}that", result2.Value);
            var result3 = _grammar.Statement2(Source("this{that"));
            Assert.AreEqual("this{that", result3.Value);
            var result4 = _grammar.Statement2(Source("this}that"));
            Assert.AreEqual("this}that", result4.Value);
        }

        [Test]
        public void VerbatimDoubleQuotes()
        {
            var result = _grammar.Expression(Source("a@\" \\\"\" \"b"));
            Assert.AreEqual("a@\" \\\"\" \"b", result.Value);

            var result2 = _grammar.Expression(Source("a@\" \\\"\"} \"b"));
            Assert.AreEqual("a@\" \\\"\"} \"b", result2.Value);
        }

        [Test]
        public void VerbatimSingleQuotes()
        {
            //@' \'' ' becomes @" \' "
            var result = _grammar.Expression(Source("a@' \\'' 'b"));
            Assert.AreEqual("a@\" \\' \"b", result.Value);

            //@' \''} ' becomes @" \'} "
            var result2 = _grammar.Expression(Source("a@' \\''} 'b"));
            Assert.AreEqual("a@\" \\'} \"b", result2.Value);

            //@' " '' ' becomes @" "" ' "
            var result3 = _grammar.Expression(Source("a@' \" '' 'b"));
            Assert.AreEqual("a@\" \"\" ' \"b", result3.Value);
        }
    }
}
