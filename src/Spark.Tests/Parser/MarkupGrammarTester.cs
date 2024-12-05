//-------------------------------------------------------------------------
// <copyright file="Constraints.cs">
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
                delegate (Position input)
                {
                    if (input.PotentialLength() == 0 || !char.IsDigit(input.Peek())) return null;
                    return new ParseResult<char>(input.Advance(1), input.Peek());
                };

            var digits = digit.Rep();

            var result = digits(Source("55407"));
            Assert.Multiple(() =>
            {
                Assert.That(result.Rest.PotentialLength(), Is.EqualTo(0));
                Assert.That(new String(result.Value.ToArray()), Is.EqualTo("55407"));
            });
        }

        [Test]
        public void TextNode()
        {
            var result = grammar.Text(Source("hello world"));
            Assert.That(result.Value.Text, Is.EqualTo("hello world"));

            var result2 = grammar.Text(Source("hello&nbsp;world"));
            Assert.That(result2.Value.Text, Is.EqualTo("hello"));
        }

        [Test]
        public void EntityNode()
        {
            var result = grammar.EntityRef(Source("&lt;"));
            Assert.That(result.Value.Name, Is.EqualTo("lt"));

            var result2 = grammar.EntityRef(Source("&lt;world"));
            Assert.That(result2.Value.Name, Is.EqualTo("lt"));

            var result3 = grammar.EntityRef(Source("hello&lt;world"));
            Assert.That(result3, Is.Null);
        }

        [Test]
        public void Rep1WontBeNone()
        {
            var parser = CharGrammar.Ch('x').Rep1();
            var three = parser(Source("xxx5"));
            Assert.That(three, Is.Not.Null);
            Assert.That(three.Value, Has.Count.EqualTo(3));

            var nada = parser(Source("yxxx"));
            Assert.That(nada, Is.Null);
        }

        [Test]
        public void EntityTextSeries()
        {
            var result = grammar.Nodes(Source("hello&nbsp;world"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(3));
            Assert.That(result.Value[0], Is.AssignableFrom(typeof(TextNode)));
            Assert.That(result.Value[1], Is.AssignableFrom(typeof(EntityNode)));
            Assert.That(result.Value[2], Is.AssignableFrom(typeof(TextNode)));
        }

        [Test]
        public void ParsingAttribute()
        {
            var result = grammar.Attribute(Source("foo=\"quad\""));
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Name, Is.EqualTo("foo"));
                Assert.That(result.Value.Value, Is.EqualTo("quad"));
            });

            var result2 = grammar.Attribute(Source("foo2='quad2'"));
            Assert.That(result2, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result2.Value.Name, Is.EqualTo("foo2"));
                Assert.That(result2.Value.Value, Is.EqualTo("quad2"));
            });

            var result3 = grammar.Attribute(Source("foo3!='quad2'"));
            Assert.That(result3, Is.Null);
        }

        [Test]
        public void ParsingElement()
        {
            var result = grammar.Element(Source("<blah>"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.Name, Is.EqualTo("blah"));
        }

        [Test]
        public void ParsingElementWithAttributes()
        {
            var result = grammar.Element(Source("<blah foo=\"quad\" omg=\"w00t\">"));
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Name, Is.EqualTo("blah"));
                Assert.That(result.Value.Attributes, Has.Count.EqualTo(2));
            });
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Attributes[0].Name, Is.EqualTo("foo"));
                Assert.That(result.Value.Attributes[0].Value, Is.EqualTo("quad"));
                Assert.That(result.Value.Attributes[1].Name, Is.EqualTo("omg"));
                Assert.That(result.Value.Attributes[1].Value, Is.EqualTo("w00t"));
            });
        }


        [Test]
        public void AttributeWithEntity()
        {
            var result = grammar.Element(Source("<blah attr=\"foo &amp; bar\" />"));

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Name, Is.EqualTo("blah"));
                Assert.That(result.Value.Attributes, Has.Count.EqualTo(1));
            });
            Assert.That(result.Value.Attributes[0].Nodes, Has.Count.EqualTo(3));
            Assert.That((result.Value.Attributes[0].Nodes[0] as TextNode).Text, Is.EqualTo("foo "));
            Assert.That((result.Value.Attributes[0].Nodes[1] as EntityNode).Name, Is.EqualTo("amp"));
            Assert.That((result.Value.Attributes[0].Nodes[2] as TextNode).Text, Is.EqualTo(" bar"));

            result = grammar.Element(Source("<blah attr='foo &amp; bar' />"));

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Name, Is.EqualTo("blah"));
                Assert.That(result.Value.Attributes, Has.Count.EqualTo(1));
            });
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Attributes[0].Nodes, Has.Count.EqualTo(3));
                Assert.That((result.Value.Attributes[0].Nodes[0] as TextNode).Text, Is.EqualTo("foo "));
                Assert.That((result.Value.Attributes[0].Nodes[1] as EntityNode).Name, Is.EqualTo("amp"));
                Assert.That((result.Value.Attributes[0].Nodes[2] as TextNode).Text, Is.EqualTo(" bar"));
            });
        }

        [Test]
        public void AttributeWithConditionalAnd()
        {
            var result = grammar.Element(Source("<blah attr=\"foo && bar\" />"));
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Name, Is.EqualTo("blah"));
                Assert.That(result.Value.Attributes, Has.Count.EqualTo(1));
            });
            Assert.That(result.Value.Attributes[0].Nodes, Has.Count.EqualTo(4));
            Assert.That((result.Value.Attributes[0].Nodes[0] as TextNode).Text, Is.EqualTo("foo "));
            Assert.That((result.Value.Attributes[0].Nodes[1] as TextNode).Text, Is.EqualTo("&"));
            Assert.That((result.Value.Attributes[0].Nodes[2] as TextNode).Text, Is.EqualTo("&"));
            Assert.That((result.Value.Attributes[0].Nodes[3] as TextNode).Text, Is.EqualTo(" bar"));

            result = grammar.Element(Source("<blah attr='foo && bar' />"));
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Name, Is.EqualTo("blah"));
                Assert.That(result.Value.Attributes, Has.Count.EqualTo(1));
            });
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Attributes[0].Nodes, Has.Count.EqualTo(4));
                Assert.That((result.Value.Attributes[0].Nodes[0] as TextNode).Text, Is.EqualTo("foo "));
                Assert.That((result.Value.Attributes[0].Nodes[1] as TextNode).Text, Is.EqualTo("&"));
                Assert.That((result.Value.Attributes[0].Nodes[2] as TextNode).Text, Is.EqualTo("&"));
                Assert.That((result.Value.Attributes[0].Nodes[3] as TextNode).Text, Is.EqualTo(" bar"));
            });
        }

        [Test]
        public void ParsingEndElement()
        {
            var result = grammar.EndElement(Source("</blah>"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.AssignableFrom(typeof(EndElementNode)));
            Assert.That(result.Value.Name, Is.EqualTo("blah"));
        }

        [Test]
        public void PassingSimpleMarkup()
        {
            var result = grammar.Nodes(Source("<foo><bar>one</bar><quad a='1' b='2'>55</quad></foo>"));
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Rest.PotentialLength(), Is.EqualTo(0));
                Assert.That(result.Value, Has.Count.EqualTo(8));
            });
            Assert.That(result.Value[4], Is.AssignableFrom(typeof(ElementNode)));
            var elt = result.Value[4] as ElementNode;
            Assert.Multiple(() =>
            {
                Assert.That(elt.Name, Is.EqualTo("quad"));
                Assert.That(elt.Attributes, Has.Count.EqualTo(2));
            });
            Assert.That(elt.Attributes[1].Value, Is.EqualTo("2"));
        }

        [Test]
        public void SelfEnding()
        {
            var result = grammar.Nodes(Source("<div><br/></div>"));
            Assert.That(result.Value[0], Is.AssignableFrom(typeof(ElementNode)));
            Assert.That(result.Value[1], Is.AssignableFrom(typeof(ElementNode)));
            Assert.That(result.Value[2], Is.AssignableFrom(typeof(EndElementNode)));

            var div = result.Value[0] as ElementNode;
            Assert.Multiple(() =>
            {
                Assert.That(div.Name, Is.EqualTo("div"));
                Assert.That(!div.IsEmptyElement);
            });

            var br = result.Value[1] as ElementNode;
            Assert.Multiple(() =>
            {
                Assert.That(br.Name, Is.EqualTo("br"));
                Assert.That(br.IsEmptyElement);
            });

            var ediv = result.Value[2] as EndElementNode;
            Assert.That(ediv.Name, Is.EqualTo("div"));
        }

        [Test]
        public void DoctypeParser()
        {
            var result =
                grammar.DoctypeDecl(
                    Source(
                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/2002/REC-xhtml1-20020801/DTD/xhtml1-strict.dtd\">"));

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Name, Is.EqualTo("html"));
                Assert.That(result.Value.ExternalId.ExternalIdType, Is.EqualTo("PUBLIC"));
                Assert.That(result.Value.ExternalId.PublicId, Is.EqualTo("-//W3C//DTD XHTML 1.0 Strict//EN"));
                Assert.That(result.Value.ExternalId.SystemId, Is.EqualTo("http://www.w3.org/TR/2002/REC-xhtml1-20020801/DTD/xhtml1-strict.dtd"));
            });

            var result2 =
                grammar.DoctypeDecl(
                    Source(
                        "<!DOCTYPE html PUBLIC \"http://www.w3.org/TR/2002/REC-xhtml1-20020801/DTD/xhtml1-strict.dtd\">"));
            Assert.That(result2, Is.Null);

            var result3 =
                grammar.DoctypeDecl(
                    Source(
                        "<!DOCTYPE html SYSTEM 'hello world'>"));
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result3.Value.Name, Is.EqualTo("html"));
                Assert.That(result3.Value.ExternalId.ExternalIdType, Is.EqualTo("SYSTEM"));
                Assert.That(result3.Value.ExternalId.SystemId, Is.EqualTo("hello world"));
            });

            var result4 =
                grammar.DoctypeDecl(
                    Source(
                        "<!DOCTYPE foo >"));
            Assert.That(result4, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result4.Value.Name, Is.EqualTo("foo"));
                Assert.That(result4.Value.ExternalId, Is.Null);
            });
        }

        [Test]
        public void CodeInText()
        {
            var result = grammar.Nodes(Source("<hello>foo${bar}ex</hello>"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(5));
            Assert.That(result.Value[2], Is.AssignableFrom(typeof(ExpressionNode)));
            var code = (ExpressionNode)result.Value[2];
            Assert.That((string)code.Code, Is.EqualTo("bar"));

            result = grammar.Nodes(Source("<hello>foo<%=baaz%>ex</hello>"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(5));
            Assert.That(result.Value[2], Is.AssignableFrom(typeof(ExpressionNode)));
            var code2 = (ExpressionNode)result.Value[2];
            Assert.That((string)code2.Code, Is.EqualTo("baaz"));

            result = grammar.Nodes(Source("<hello href='${one}' class=\"<%=two%>\"/>"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(1));
            Assert.That(result.Value[0], Is.AssignableFrom(typeof(ElementNode)));
            var elt = (ElementNode)result.Value[0];
            Assert.Multiple(() =>
            {
                Assert.That((string)((ExpressionNode)elt.Attributes[0].Nodes[0]).Code, Is.EqualTo("one"));
                Assert.That((string)((ExpressionNode)elt.Attributes[1].Nodes[0]).Code, Is.EqualTo("two"));
            });

        }

        [Test]
        public void AspxStyleOutputInText()
        {
            var result = grammar.Nodes(Source("<hello>foo<%=bar%>ex</hello>"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(5));
            Assert.That(result.Value[2], Is.AssignableFrom(typeof(ExpressionNode)));
            var code = result.Value[2] as ExpressionNode;
            Assert.Multiple(() =>
            {
                Assert.That((string)code.Code, Is.EqualTo("bar"));


                Assert.That(((TextNode)result.Value[1]).Text, Is.EqualTo("foo"));
                Assert.That(((TextNode)result.Value[3]).Text, Is.EqualTo("ex"));
            });
        }

        [Test]
        public void CommentParser()
        {
            var result = grammar.Comment(Source("<!-- hello world -->"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.Text, Is.EqualTo(" hello world "));

            var result2 = grammar.Comment(Source("<!-- hello-world -->"));
            Assert.That(result2, Is.Not.Null);
            Assert.That(result2.Value.Text, Is.EqualTo(" hello-world "));

            var result3 = grammar.Comment(Source("<!-- hello--world -->"));
            Assert.That(result3, Is.Null);
        }

        [Test]
        public void CodeStatementsPercentSyntax()
        {
            var direct = grammar.Statement(Source("<%int x = 5;%>"));
            Assert.That((string)direct.Value.Code, Is.EqualTo("int x = 5;"));

            var result = grammar.Nodes(Source("<div>hello <%int x = 5;%> world</div>"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(5));
            var stmt = result.Value[2] as StatementNode;
            Assert.Multiple(() =>
            {
                Assert.That(stmt, Is.Not.Null);
                Assert.That((string)stmt.Code, Is.EqualTo("int x = 5;"));
            });
        }

        [Test]
        public void CodeStatementsHashSyntax()
        {
            var direct = grammar.Statement(Source("\n#int x = 5;\n"));
            Assert.That((string)direct.Value.Code, Is.EqualTo("int x = 5;"));

            var result = grammar.Nodes(Source("<div>hello\n #int x = 5;\n world</div>"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(5));
            var stmt = result.Value[2] as StatementNode;
            Assert.Multiple(() =>
            {
                Assert.That(stmt, Is.Not.Null);
                Assert.That((string)stmt.Code, Is.EqualTo("int x = 5;"));
            });
        }

        [Test]
        public void SpecialCharactersInAttributes()
        {
            var attr1 = grammar.Attribute(Source("foo=\"bar$('hello')\""));
            Assert.That(attr1.Value.Value, Is.EqualTo("bar$('hello')"));

            var attr2 = grammar.Attribute(Source("foo=\"$('#hello')\""));
            Assert.That(attr2.Value.Value, Is.EqualTo("$('#hello')"));

            var attr3 = grammar.Attribute(Source("foo='#hello'"));
            Assert.That(attr3.Value.Value, Is.EqualTo("#hello"));
        }

        [Test]
        public void JQueryIdSelectorInAttribute()
        {
            var attr1 = grammar.Attribute(Source("foo='javascript:$(\"#diff\").hide()'"));
            Assert.That(attr1.Value.Value, Is.EqualTo("javascript:$(\"#diff\").hide()"));

            var attr2 = grammar.Attribute(Source("foo=\"javascript:$('#diff').hide()\""));
            Assert.That(attr2.Value.Value, Is.EqualTo("javascript:$('#diff').hide()"));
        }

        [Test]
        public void JQueryIdSelectorInText()
        {
            var nodes1 = grammar.Nodes(Source("<script>\r\n$(\"#diff\").hide();\r\n</script>"));
            Assert.Multiple(() =>
            {
                Assert.That(nodes1.Value, Has.Count.EqualTo(3));
                Assert.That(((TextNode)nodes1.Value[1]).Text, Tests.Contains.InOrder("$(\"#diff\").hide();"));
            });

            var nodes2 = grammar.Nodes(Source("<script>\r\n$('#diff').hide();\r\n</script>"));
            Assert.Multiple(() =>
            {
                Assert.That(nodes2.Value, Has.Count.EqualTo(3));
                Assert.That(((TextNode)nodes2.Value[1]).Text, Tests.Contains.InOrder("$('#diff').hide();"));
            });
        }


        [Test]
        public void HashStatementMustBeFirstNonWhitespaceCharacter()
        {
            var nodes1 = grammar.Nodes(Source("<p>abc\r\n \t#Logger.Warn('Hello World');\r\ndef</p>"));
            Assert.Multiple(() =>
            {
                Assert.That(nodes1.Value, Has.Count.EqualTo(5));
                Assert.That((string)((StatementNode)nodes1.Value[2]).Code, Is.EqualTo("Logger.Warn(\"Hello World\");"));
            });

            var nodes2 = grammar.Nodes(Source("<p>abc\r\n \t x#Logger.Warn('Hello World');\r\ndef</p>"));
            Assert.Multiple(() =>
            {
                Assert.That(nodes2.Value, Has.Count.EqualTo(3));
                Assert.That((string)((TextNode)nodes2.Value[1]).Text, Is.EqualTo("abc\r\n \t x#Logger.Warn('Hello World');\r\ndef"));
            });
        }

        [Test]
        public void ConditionalSyntaxInAttributes()
        {
            var attr = grammar.Attribute(Source("foo=\"one?{true}\""));
            Assert.Multiple(() =>
            {
                Assert.That(attr.Rest.PotentialLength(), Is.EqualTo(0));
                Assert.That(attr.Value.Name, Is.EqualTo("foo"));
                Assert.That(attr.Value.Nodes, Has.Count.EqualTo(2));
                Assert.That(attr.Value.Value, Is.EqualTo("one?{true}"));
                Assert.That(((TextNode)attr.Value.Nodes[0]).Text, Is.EqualTo("one"));
                Assert.That((string)((ConditionNode)attr.Value.Nodes[1]).Code, Is.EqualTo("true"));
            });
        }

        [Test]
        public void XMLDeclParser()
        {
            var result =
                grammar.XMLDecl(
                    Source("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"));
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Encoding, Is.EqualTo("UTF-8"));
                Assert.That(result.Value.Standalone, Is.Null);
            });

            result =
                grammar.XMLDecl(
                    Source("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone='no'  ?>"));

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Encoding, Is.EqualTo("UTF-8"));
                Assert.That(result.Value.Standalone, Is.EqualTo("no"));
            });

            result =
                grammar.XMLDecl(
                    Source("<?xml version=\"1.0\" standalone=\"yes\"  ?>"));

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Encoding, Is.Null);
                Assert.That(result.Value.Standalone, Is.EqualTo("yes"));
            });
        }

        [Test]
        public void ProcessingInstructionWontParseXMLDecl()
        {
            var result =
                grammar.ProcessingInstruction(
                    Source("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"));
            Assert.That(result, Is.Null);
        }


        [Test]
        public void ProcessingInstruction()
        {
            var result = grammar.ProcessingInstruction(
                    Source("<?foo?>"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Name, Is.EqualTo("foo"));
                Assert.That(string.IsNullOrEmpty(result.Value.Body));
            });

            result = grammar.ProcessingInstruction(
                    Source("<?php hello ?>"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Name, Is.EqualTo("php"));
                Assert.That(result.Value.Body, Is.EqualTo("hello "));
            });
        }

        [Test]
        public void LessThanCanBeUsedAsText()
        {
            var result = grammar.Nodes(
                    Source("<p class=\"three<four\">One<Two</p>"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Value, Has.Count.EqualTo(5));
                Assert.That(((TextNode)result.Value[2]).Text, Is.EqualTo("<"));
            });

            var elt = (ElementNode)result.Value[0];
            Assert.Multiple(() =>
            {
                Assert.That(elt.Attributes[0].Nodes, Has.Count.EqualTo(3));
                Assert.That(((TextNode)elt.Attributes[0].Nodes[1]).Text, Is.EqualTo("<"));
            });
        }

        [Test]
        public void StatementAtStartOfFile()
        {
            var result1 = grammar.Nodes(
                Source("#alpha\r\n"));
            Assert.That(result1.Value, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(result1.Value[0], Is.InstanceOf(typeof(StatementNode)));
                Assert.That((string)((StatementNode)result1.Value[0]).Code, Is.EqualTo("alpha"));
                Assert.That(result1.Value[1], Is.InstanceOf(typeof(TextNode)));
                Assert.That(((TextNode)result1.Value[1]).Text, Is.EqualTo("\r\n"));
            });

            var result2 = grammar.Nodes(
                Source("#alpha\r\ntext\r\n#beta"));
            Assert.That(result2.Value, Has.Count.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(result2.Value[0], Is.InstanceOf(typeof(StatementNode)));
                Assert.That((string)((StatementNode)result2.Value[0]).Code, Is.EqualTo("alpha"));
                Assert.That(result2.Value[1], Is.InstanceOf(typeof(TextNode)));
                Assert.That(((TextNode)result2.Value[1]).Text, Is.EqualTo("\r\ntext"));
                Assert.That(result2.Value[2], Is.InstanceOf(typeof(StatementNode)));
                Assert.That((string)((StatementNode)result2.Value[2]).Code, Is.EqualTo("beta"));
            });

            var result3 = grammar.Nodes(
                Source("\r\n#alpha\r\ntext\r\n#beta\r\n"));
            Assert.That(result3.Value, Has.Count.EqualTo(4));
            Assert.Multiple(() =>
            {
                Assert.That(result3.Value[0], Is.InstanceOf(typeof(StatementNode)));
                Assert.That((string)((StatementNode)result3.Value[0]).Code, Is.EqualTo("alpha"));
                Assert.That(result3.Value[1], Is.InstanceOf(typeof(TextNode)));
                Assert.That(((TextNode)result3.Value[1]).Text, Is.EqualTo("\r\ntext"));
                Assert.That(result3.Value[2], Is.InstanceOf(typeof(StatementNode)));
                Assert.That((string)((StatementNode)result3.Value[2]).Code, Is.EqualTo("beta"));
                Assert.That(result3.Value[3], Is.InstanceOf(typeof(TextNode)));
                Assert.That(((TextNode)result3.Value[3]).Text, Is.EqualTo("\r\n"));
            });
        }
    }
}

