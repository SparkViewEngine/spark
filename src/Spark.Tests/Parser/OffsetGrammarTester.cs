using System.Linq;
using NUnit.Framework;
using Shouldly;
using Spark.Parser;
using Spark.Parser.Markup;
using Spark.Parser.Offset;

namespace Spark.Tests.Parser
{
    [TestFixture]
    public class OffsetGrammarTester
    {
        private OffsetGrammar grammar;

        [SetUp]
        public void Init()
        {
            grammar = new OffsetGrammar();
        }

        private Position Source(string content)
        {
            return new Position(new SourceContext(content));
        }

        [Test]
        public void IndentationMatchesWhitespace()
        {
            var result = grammar.Indentation(Source("      45"));
            result.Value.Whitespace.ShouldBe("      ");
        }

        [Test]
        public void LinesThatAreOnlyWhitespaceAreNotIndentation()
        {
            var result = grammar.Indentation(Source("    \r\n  45"));
            result.ShouldBe(null);
        }

        [Test]
        public void ElementNameIsRecognized()
        {
            var result = grammar.OffsetElement(Source("hello"));
            result.Value.Name.ShouldBe("hello");
            result.Value.Attributes.Count.ShouldBe(0);
        }

        [Test]
        public void IdAttributeIsDetected()
        {
            var result = grammar.OffsetElement(Source("hello#foo"));
            result.Value.Name.ShouldBe("hello");
            result.Value.Attributes.Count.ShouldBe(1);
            result.Value.Attributes[0].Name.ShouldBe("id");
            result.Value.Attributes[0].Value.ShouldBe("foo");
        }

        [Test]
        public void ClassIsDetected()
        {
            var result = grammar.OffsetElement(Source("hello.foo"));
            result.Value.Name.ShouldBe("hello");
            result.Value.Attributes.Count.ShouldBe(1);
            result.Value.Attributes[0].Name.ShouldBe("class");
            result.Value.Attributes[0].Value.ShouldBe("foo");
        }

        [Test]
        public void ClassAndIdAndClassIsDetected()
        {
            var result = grammar.OffsetElement(Source("hello.one#two.three"));
            result.Value.Name.ShouldBe("hello");
            result.Value.Attributes.Count.ShouldBe(2);
            result.Value.Attributes[0].Name.ShouldBe("id");
            result.Value.Attributes[0].Value.ShouldBe("two");
            result.Value.Attributes[1].Name.ShouldBe("class");
            result.Value.Attributes[1].Value.ShouldBe("one three");
        }

        [Test]
        public void DivIsImplied()
        {
            grammar.OffsetElement(Source("#foo")).Value.Name.ShouldBe("div");
            grammar.OffsetElement(Source(".bar")).Value.Name.ShouldBe("div");
            grammar.OffsetElement(Source("#foo.bar")).Value.Name.ShouldBe("div");
            grammar.OffsetElement(Source(".bar#foo")).Value.Name.ShouldBe("div");
        }

        [Test]
        public void AttributesMayFollowElement()
        {
            var element = grammar.OffsetElement(Source("this.and#that one='two' three='four'"));
            element.Value.Attributes.Count.ShouldBe(4);
            element.Value.Attributes[0].Name.ShouldBe("id");
            element.Value.Attributes[1].Name.ShouldBe("class");
            element.Value.Attributes[2].Name.ShouldBe("one");
            element.Value.Attributes[2].Value.ShouldBe("two");
            element.Value.Attributes[3].Name.ShouldBe("three");
            element.Value.Attributes[3].Value.ShouldBe("four");
        }

        [Test]
        public void TextFollowsPipe()
        {
            grammar.OffsetTexts(Source("| hello")).Value.Count.ShouldBe(1);
            grammar.OffsetTexts(Source("| hello")).Value.OfType<TextNode>().Single().Text.ShouldBe(" hello");
        }


        [Test]
        public void TextMayHaveExpressions()
        {
            var nodes = grammar.OffsetTexts(Source("| hello ${5} world")).Value;
            nodes.Count.ShouldBe(3);
            nodes.OfType<TextNode>().First().Text.ShouldBe(" hello ");
            nodes.OfType<ExpressionNode>().Single().Code.ToString().ShouldBe("5");
            nodes.OfType<TextNode>().Last().Text.ShouldBe(" world");
        }

        [Test]
        public void ExpressionFollowsEqual()
        {
            grammar.OffsetExpression(Source("=42")).Value.Code.ToString().ShouldBe("42");
        }
        [Test]
        public void ExpressionEndsAtLine()
        {
            grammar.OffsetExpression(Source("=42\r\n4")).Value.Code.ToString().ShouldBe("42");
        }

        [Test]
        public void ExpressionMayWrapIfEncased()
        {
            grammar.OffsetExpression(Source("={42\r\n4}")).Value.Code.ToString().ShouldBe("{42\r\n4}");
            grammar.OffsetExpression(Source("=(42\r\n4)")).Value.Code.ToString().ShouldBe("(42\r\n4)");
            grammar.OffsetExpression(Source("='42\r\n4'")).Value.Code.ToString().ShouldBe("\"42\r\n4\"");
        }


        [Test]
        public void StatementFollowsDash()
        {
            grammar.OffsetStatement(Source("- for(var x in 42) { \r\n")).Value.Code.ToString().ShouldBe(" for(var x in 42) { ");
        }
        [Test]
        public void StatementCanAlsoBeAtBraced()
        {
            grammar.OffsetStatement(Source("@{ for(var x in 42) { \r\n yadda }a}b}c")).Value.Code.ToString().ShouldBe(" for(var x in 42) { \r\n yadda }a");
        }

        [Test]
        public void Altogether()
        {
            var nodes = grammar.OffsetNodes(Source(
@"

p.top
  span|hello world
  ul#nav
    li|yep

")).Value;
            nodes.Count().ShouldBe(10);
        }
    }
}
