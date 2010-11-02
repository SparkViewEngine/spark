//-------------------------------------------------------------------------
// <copyright file="Constraints.cs">
// Copyright 2008-2010 Louis DeJardin - http://whereslou.com
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
// </copyright>
// <author>Louis DeJardin</author>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

namespace Spark.Tests.Parser
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using Spark.Parser;
    using Spark.Parser.Markup;

    [TestFixture]
    public class MarkupGrammarTester
    {
        private MarkupGrammar grammar;

        [SetUp]
        public void Init()
        {
            grammar = new MarkupGrammar();
        }

        private Position Source(string content)
        {
            return new Position(new SourceContext(content));
        }

        [Test]
        public void RepDigits()
        {
            ParseAction<char> digit =
                delegate(Position input)
                {
                    if (input.PotentialLength() == 0 || !char.IsDigit(input.Peek())) return null;
                    return new ParseResult<char>(input.Advance(1), input.Peek());
                };

            var digits = digit.Rep();

            var result = digits(Source("55407"));
            Assert.AreEqual(0, result.Rest.PotentialLength());
            Assert.AreEqual("55407", new String(result.Value.ToArray()));
        }

        [Test]
        public void TextNode()
        {
            var result = grammar.Text(Source("hello world"));
            Assert.AreEqual("hello world", result.Value.Text);

            var result2 = grammar.Text(Source("hello&nbsp;world"));
            Assert.AreEqual("hello", result2.Value.Text);
        }

        [Test]
        public void EntityNode()
        {
            var result = grammar.EntityRef(Source("&lt;"));
            Assert.AreEqual("lt", result.Value.Name);

            var result2 = grammar.EntityRef(Source("&lt;world"));
            Assert.AreEqual("lt", result2.Value.Name);

            var result3 = grammar.EntityRef(Source("hello&lt;world"));
            Assert.IsNull(result3);
        }

        [Test]
        public void Rep1WontBeNone()
        {
            var parser = CharGrammar.Ch('x').Rep1();
            var three = parser(Source("xxx5"));
            Assert.IsNotNull(three);
            Assert.AreEqual(3, three.Value.Count);

            var nada = parser(Source("yxxx"));
            Assert.IsNull(nada);
        }

        [Test]
        public void EntityTextSeries()
        {
            var result = grammar.Nodes(Source("hello&nbsp;world"));
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Value.Count);
            Assert.IsAssignableFrom(typeof(TextNode), result.Value[0]);
            Assert.IsAssignableFrom(typeof(EntityNode), result.Value[1]);
            Assert.IsAssignableFrom(typeof(TextNode), result.Value[2]);
        }

        [Test]
        public void ParsingAttribute()
        {
            var result = grammar.Attribute(Source("foo=\"quad\""));
            Assert.IsNotNull(result);
            Assert.AreEqual("foo", result.Value.Name);
            Assert.AreEqual("quad", result.Value.Value);

            var result2 = grammar.Attribute(Source("foo2='quad2'"));
            Assert.IsNotNull(result2);
            Assert.AreEqual("foo2", result2.Value.Name);
            Assert.AreEqual("quad2", result2.Value.Value);

            var result3 = grammar.Attribute(Source("foo3!='quad2'"));
            Assert.IsNull(result3);
        }

        [Test]
        public void ParsingElement()
        {
            var result = grammar.Element(Source("<blah>"));
            Assert.IsNotNull(result);
            Assert.AreEqual("blah", result.Value.Name);
        }

        [Test]
        public void ParsingElementWithAttributes()
        {
            var result = grammar.Element(Source("<blah foo=\"quad\" omg=\"w00t\">"));
            Assert.IsNotNull(result);
            Assert.AreEqual("blah", result.Value.Name);
            Assert.AreEqual(2, result.Value.Attributes.Count);
            Assert.AreEqual("foo", result.Value.Attributes[0].Name);
            Assert.AreEqual("quad", result.Value.Attributes[0].Value);
            Assert.AreEqual("omg", result.Value.Attributes[1].Name);
            Assert.AreEqual("w00t", result.Value.Attributes[1].Value);
        }


        [Test]
        public void AttributeWithEntity()
        {
            var result = grammar.Element(Source("<blah attr=\"foo &amp; bar\" />"));

            Assert.IsNotNull(result);
            Assert.AreEqual("blah", result.Value.Name);
            Assert.AreEqual(1, result.Value.Attributes.Count);
            Assert.AreEqual(3, result.Value.Attributes[0].Nodes.Count);
            Assert.AreEqual("foo ", (result.Value.Attributes[0].Nodes[0] as TextNode).Text);
            Assert.AreEqual("amp", (result.Value.Attributes[0].Nodes[1] as EntityNode).Name);
            Assert.AreEqual(" bar", (result.Value.Attributes[0].Nodes[2] as TextNode).Text);

            result = grammar.Element(Source("<blah attr='foo &amp; bar' />"));

            Assert.IsNotNull(result);
            Assert.AreEqual("blah", result.Value.Name);
            Assert.AreEqual(1, result.Value.Attributes.Count);
            Assert.AreEqual(3, result.Value.Attributes[0].Nodes.Count);
            Assert.AreEqual("foo ", (result.Value.Attributes[0].Nodes[0] as TextNode).Text);
            Assert.AreEqual("amp", (result.Value.Attributes[0].Nodes[1] as EntityNode).Name);
            Assert.AreEqual(" bar", (result.Value.Attributes[0].Nodes[2] as TextNode).Text);
        }

        [Test]
        public void AttributeWithConditionalAnd()
        {
            var result = grammar.Element(Source("<blah attr=\"foo && bar\" />"));
            Assert.IsNotNull(result);
            Assert.AreEqual("blah", result.Value.Name);
            Assert.AreEqual(1, result.Value.Attributes.Count);
            Assert.AreEqual(4, result.Value.Attributes[0].Nodes.Count);
            Assert.AreEqual("foo ", (result.Value.Attributes[0].Nodes[0] as TextNode).Text);
            Assert.AreEqual("&", (result.Value.Attributes[0].Nodes[1] as TextNode).Text);
            Assert.AreEqual("&", (result.Value.Attributes[0].Nodes[2] as TextNode).Text);
            Assert.AreEqual(" bar", (result.Value.Attributes[0].Nodes[3] as TextNode).Text);

            result = grammar.Element(Source("<blah attr='foo && bar' />"));
            Assert.IsNotNull(result);
            Assert.AreEqual("blah", result.Value.Name);
            Assert.AreEqual(1, result.Value.Attributes.Count);
            Assert.AreEqual(4, result.Value.Attributes[0].Nodes.Count);
            Assert.AreEqual("foo ", (result.Value.Attributes[0].Nodes[0] as TextNode).Text);
            Assert.AreEqual("&", (result.Value.Attributes[0].Nodes[1] as TextNode).Text);
            Assert.AreEqual("&", (result.Value.Attributes[0].Nodes[2] as TextNode).Text);
            Assert.AreEqual(" bar", (result.Value.Attributes[0].Nodes[3] as TextNode).Text);
        }

        [Test]
        public void ParsingEndElement()
        {
            var result = grammar.EndElement(Source("</blah>"));
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom(typeof(EndElementNode), result.Value);
            Assert.AreEqual("blah", result.Value.Name);
        }

        [Test]
        public void PassingSimpleMarkup()
        {
            var result = grammar.Nodes(Source("<foo><bar>one</bar><quad a='1' b='2'>55</quad></foo>"));
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Rest.PotentialLength());
            Assert.AreEqual(8, result.Value.Count);
            Assert.IsAssignableFrom(typeof(ElementNode), result.Value[4]);
            var elt = result.Value[4] as ElementNode;
            Assert.AreEqual("quad", elt.Name);
            Assert.AreEqual(2, elt.Attributes.Count);
            Assert.AreEqual("2", elt.Attributes[1].Value);
        }

        [Test]
        public void SelfEnding()
        {
            var result = grammar.Nodes(Source("<div><br/></div>"));
            Assert.IsAssignableFrom(typeof(ElementNode), result.Value[0]);
            Assert.IsAssignableFrom(typeof(ElementNode), result.Value[1]);
            Assert.IsAssignableFrom(typeof(EndElementNode), result.Value[2]);

            var div = result.Value[0] as ElementNode;
            Assert.AreEqual("div", div.Name);
            Assert.That(!div.IsEmptyElement);

            var br = result.Value[1] as ElementNode;
            Assert.AreEqual("br", br.Name);
            Assert.That(br.IsEmptyElement);

            var ediv = result.Value[2] as EndElementNode;
            Assert.AreEqual("div", ediv.Name);
        }

        [Test]
        public void DoctypeParser()
        {
            var result =
                grammar.DoctypeDecl(
                    Source(
                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/2002/REC-xhtml1-20020801/DTD/xhtml1-strict.dtd\">"));

            Assert.IsNotNull(result);
            Assert.AreEqual("html", result.Value.Name);
            Assert.AreEqual("PUBLIC", result.Value.ExternalId.ExternalIdType);
            Assert.AreEqual("-//W3C//DTD XHTML 1.0 Strict//EN", result.Value.ExternalId.PublicId);
            Assert.AreEqual("http://www.w3.org/TR/2002/REC-xhtml1-20020801/DTD/xhtml1-strict.dtd", result.Value.ExternalId.SystemId);

            var result2 =
                grammar.DoctypeDecl(
                    Source(
                        "<!DOCTYPE html PUBLIC \"http://www.w3.org/TR/2002/REC-xhtml1-20020801/DTD/xhtml1-strict.dtd\">"));
            Assert.IsNull(result2);

            var result3 =
                grammar.DoctypeDecl(
                    Source(
                        "<!DOCTYPE html SYSTEM 'hello world'>"));
            Assert.IsNotNull(result);
            Assert.AreEqual("html", result3.Value.Name);
            Assert.AreEqual("SYSTEM", result3.Value.ExternalId.ExternalIdType);
            Assert.AreEqual("hello world", result3.Value.ExternalId.SystemId);

            var result4 =
                grammar.DoctypeDecl(
                    Source(
                        "<!DOCTYPE foo >"));
            Assert.IsNotNull(result4);
            Assert.AreEqual("foo", result4.Value.Name);
            Assert.IsNull(result4.Value.ExternalId);
        }

        [Test]
        public void CodeInText()
        {
            var result = grammar.Nodes(Source("<hello>foo${bar}ex</hello>"));
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Value.Count);
            Assert.IsAssignableFrom(typeof(ExpressionNode), result.Value[2]);
            var code = (ExpressionNode)result.Value[2];
            Assert.AreEqual("bar", (string)code.Code);

            result = grammar.Nodes(Source("<hello>foo<%=baaz%>ex</hello>"));
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Value.Count);
            Assert.IsAssignableFrom(typeof(ExpressionNode), result.Value[2]);
            var code2 = (ExpressionNode)result.Value[2];
            Assert.AreEqual("baaz", (string)code2.Code);

            result = grammar.Nodes(Source("<hello href='${one}' class=\"<%=two%>\"/>"));
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Value.Count);
            Assert.IsAssignableFrom(typeof(ElementNode), result.Value[0]);
            var elt = (ElementNode)result.Value[0];
            Assert.AreEqual("one", (string)((ExpressionNode)elt.Attributes[0].Nodes[0]).Code);
            Assert.AreEqual("two", (string)((ExpressionNode)elt.Attributes[1].Nodes[0]).Code);

        }

        [Test]
        public void AspxStyleOutputInText()
        {
            var result = grammar.Nodes(Source("<hello>foo<%=bar%>ex</hello>"));
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Value.Count);
            Assert.IsAssignableFrom(typeof(ExpressionNode), result.Value[2]);
            var code = result.Value[2] as ExpressionNode;
            Assert.AreEqual("bar", (string)code.Code);


            Assert.AreEqual("foo", ((TextNode)result.Value[1]).Text);
            Assert.AreEqual("ex", ((TextNode)result.Value[3]).Text);
        }

        [Test]
        public void CommentParser()
        {
            var result = grammar.Comment(Source("<!-- hello world -->"));
            Assert.IsNotNull(result);
            Assert.AreEqual(" hello world ", result.Value.Text);

            var result2 = grammar.Comment(Source("<!-- hello-world -->"));
            Assert.IsNotNull(result2);
            Assert.AreEqual(" hello-world ", result2.Value.Text);

            var result3 = grammar.Comment(Source("<!-- hello--world -->"));
            Assert.IsNull(result3);
        }

        [Test]
        public void CodeStatementsPercentSyntax()
        {
            var direct = grammar.Statement(Source("<%int x = 5;%>"));
            Assert.AreEqual("int x = 5;", (string)direct.Value.Code);

            var result = grammar.Nodes(Source("<div>hello <%int x = 5;%> world</div>"));
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Value.Count);
            var stmt = result.Value[2] as StatementNode;
            Assert.IsNotNull(stmt);
            Assert.AreEqual("int x = 5;", (string)stmt.Code);
        }

        [Test]
        public void CodeStatementsHashSyntax()
        {
            var direct = grammar.Statement(Source("\n#int x = 5;\n"));
            Assert.AreEqual("int x = 5;", (string)direct.Value.Code);

            var result = grammar.Nodes(Source("<div>hello\n #int x = 5;\n world</div>"));
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Value.Count);
            var stmt = result.Value[2] as StatementNode;
            Assert.IsNotNull(stmt);
            Assert.AreEqual("int x = 5;", (string)stmt.Code);
        }

        [Test]
        public void SpecialCharactersInAttributes()
        {
            var attr1 = grammar.Attribute(Source("foo=\"bar$('hello')\""));
            Assert.AreEqual("bar$('hello')", attr1.Value.Value);

            var attr2 = grammar.Attribute(Source("foo=\"$('#hello')\""));
            Assert.AreEqual("$('#hello')", attr2.Value.Value);

            var attr3 = grammar.Attribute(Source("foo='#hello'"));
            Assert.AreEqual("#hello", attr3.Value.Value);
        }

        [Test]
        public void JQueryIdSelectorInAttribute()
        {
            var attr1 = grammar.Attribute(Source("foo='javascript:$(\"#diff\").hide()'"));
            Assert.AreEqual("javascript:$(\"#diff\").hide()", attr1.Value.Value);

            var attr2 = grammar.Attribute(Source("foo=\"javascript:$('#diff').hide()\""));
            Assert.AreEqual("javascript:$('#diff').hide()", attr2.Value.Value);
        }

        [Test]
        public void JQueryIdSelectorInText()
        {
            var nodes1 = grammar.Nodes(Source("<script>\r\n$(\"#diff\").hide();\r\n</script>"));
            Assert.AreEqual(3, nodes1.Value.Count);
            Assert.That(((TextNode)nodes1.Value[1]).Text, Contains.InOrder("$(\"#diff\").hide();"));

            var nodes2 = grammar.Nodes(Source("<script>\r\n$('#diff').hide();\r\n</script>"));
            Assert.AreEqual(3, nodes2.Value.Count);
            Assert.That(((TextNode)nodes2.Value[1]).Text, Contains.InOrder("$('#diff').hide();"));
        }


        [Test]
        public void HashStatementMustBeFirstNonWhitespaceCharacter()
        {
            var nodes1 = grammar.Nodes(Source("<p>abc\r\n \t#Logger.Warn('Hello World');\r\ndef</p>"));
            Assert.AreEqual(5, nodes1.Value.Count);
            Assert.AreEqual("Logger.Warn(\"Hello World\");", (string)((StatementNode)nodes1.Value[2]).Code);

            var nodes2 = grammar.Nodes(Source("<p>abc\r\n \t x#Logger.Warn('Hello World');\r\ndef</p>"));
            Assert.AreEqual(3, nodes2.Value.Count);
            Assert.AreEqual("abc\r\n \t x#Logger.Warn('Hello World');\r\ndef", (string)((TextNode)nodes2.Value[1]).Text);
        }

        [Test]
        public void ConditionalSyntaxInAttributes()
        {
            var attr = grammar.Attribute(Source("foo=\"one?{true}\""));
            Assert.AreEqual(0, attr.Rest.PotentialLength());
            Assert.AreEqual("foo", attr.Value.Name);
            Assert.AreEqual(2, attr.Value.Nodes.Count);
            Assert.AreEqual("one?{true}", attr.Value.Value);
            Assert.AreEqual("one", ((TextNode)attr.Value.Nodes[0]).Text);
            Assert.AreEqual("true", (string)((ConditionNode)attr.Value.Nodes[1]).Code);
        }

        [Test]
        public void XMLDeclParser()
        {
            var result =
                grammar.XMLDecl(
                    Source("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"));
            Assert.IsNotNull(result);
            Assert.AreEqual("UTF-8", result.Value.Encoding);
            Assert.IsNull(result.Value.Standalone);

            result =
                grammar.XMLDecl(
                    Source("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone='no'  ?>"));
         
            Assert.IsNotNull(result);
            Assert.AreEqual("UTF-8", result.Value.Encoding);
            Assert.AreEqual("no", result.Value.Standalone);

            result =
                grammar.XMLDecl(
                    Source("<?xml version=\"1.0\" standalone=\"yes\"  ?>"));

            Assert.IsNotNull(result);
            Assert.IsNull(result.Value.Encoding);
            Assert.AreEqual("yes", result.Value.Standalone);
        }

        [Test]
        public void ProcessingInstructionWontParseXMLDecl()
        {
            var result =
                grammar.ProcessingInstruction(
                    Source("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"));
            Assert.IsNull(result);
        }


        [Test]
        public void ProcessingInstruction()
        {
            var result = grammar.ProcessingInstruction(
                    Source("<?foo?>"));

            Assert.AreEqual("foo", result.Value.Name);
            Assert.That(string.IsNullOrEmpty(result.Value.Body));

            result = grammar.ProcessingInstruction(
                    Source("<?php hello ?>"));

            Assert.AreEqual("php", result.Value.Name);
            Assert.AreEqual("hello ", result.Value.Body);
        }

        [Test]
        public void LessThanCanBeUsedAsText()
        {
            var result = grammar.Nodes(
                    Source("<p class=\"three<four\">One<Two</p>"));

            Assert.AreEqual(5, result.Value.Count);
            Assert.AreEqual("<", ((TextNode)result.Value[2]).Text);

            var elt = (ElementNode) result.Value[0];
            Assert.AreEqual(3, elt.Attributes[0].Nodes.Count);
            Assert.AreEqual("<", ((TextNode)elt.Attributes[0].Nodes[1]).Text);
        }

        [Test]
        public void StatementAtStartOfFile()
        {
            var result1 = grammar.Nodes(
                Source("#alpha\r\n"));
            Assert.AreEqual(2, result1.Value.Count);
            Assert.IsInstanceOfType(typeof(StatementNode), result1.Value[0]);
            Assert.AreEqual("alpha", (string)((StatementNode)result1.Value[0]).Code);
            Assert.IsInstanceOfType(typeof(TextNode), result1.Value[1]);
            Assert.AreEqual("\r\n", ((TextNode)result1.Value[1]).Text);

            var result2 = grammar.Nodes(
                Source("#alpha\r\ntext\r\n#beta"));
            Assert.AreEqual(3, result2.Value.Count);
            Assert.IsInstanceOfType(typeof(StatementNode), result2.Value[0]);
            Assert.AreEqual("alpha", (string)((StatementNode)result2.Value[0]).Code);
            Assert.IsInstanceOfType(typeof(TextNode), result2.Value[1]);
            Assert.AreEqual("\r\ntext", ((TextNode)result2.Value[1]).Text);
            Assert.IsInstanceOfType(typeof(StatementNode), result2.Value[2]);
            Assert.AreEqual("beta", (string)((StatementNode)result2.Value[2]).Code);

            var result3 = grammar.Nodes(
                Source("\r\n#alpha\r\ntext\r\n#beta\r\n"));
            Assert.AreEqual(4, result3.Value.Count);
            Assert.IsInstanceOfType(typeof(StatementNode), result3.Value[0]);
            Assert.AreEqual("alpha", (string)((StatementNode)result3.Value[0]).Code);
            Assert.IsInstanceOfType(typeof(TextNode), result3.Value[1]);
            Assert.AreEqual("\r\ntext", ((TextNode)result3.Value[1]).Text);
            Assert.IsInstanceOfType(typeof(StatementNode), result3.Value[2]);
            Assert.AreEqual("beta", (string)((StatementNode)result3.Value[2]).Code);
            Assert.IsInstanceOfType(typeof(TextNode), result3.Value[3]);
            Assert.AreEqual("\r\n", ((TextNode)result3.Value[3]).Text);
        }
    }
}

