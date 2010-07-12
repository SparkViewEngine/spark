using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.Parser.Markup;
using SparkSense.Parsing;
using System;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class ParseSingleNodeTests
    {
        [Test]
        public void ShouldReturnElementNodeGivenPositionInsideAnyElement()
        {
            var node = SparkSyntax.ParseNode("<div></div>", position: 1);

            Assert.That(node, Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(((ElementNode)node).Name, Is.EqualTo("div"));
        }

        [Test]
        public void ShouldReturnNullGivenPositionAtTheBeginning()
        {
            var node = SparkSyntax.ParseNode("<div></div>", position: 0);

            Assert.That(node, Is.Null);
        }

        [Test]
        public void ShouldReturnNullGivenPositionAtTheEnd()
        {
            var node = SparkSyntax.ParseNode("<div></div>", position: 11);

            Assert.That(node, Is.Null);
        }

        [Test]
        public void ShouldReturnNullGivenPositionInAnEndElement()
        {
            var node = SparkSyntax.ParseNode("<div></div>", position: 10);

            Assert.That(node, Is.Null);
        }

        [Test]
        public void ShouldReturnNullGivenPositionBetweenNodes()
        {
            var node = SparkSyntax.ParseNode("<div><use content='main'/></div>", position: 5);

            Assert.That(node, Is.Null);
        }

        [Test]
        public void ShouldReturnNullGivenPositionAtEndOfLine()
        {
            var node = SparkSyntax.ParseNode(String.Format("<div><use content='main'/>{0}</div>", Environment.NewLine), position: 27);

            Assert.That(node, Is.Null);
        }

        [Test]
        public void ShouldReturnElementNodeGivenPositionInsideACompleteElement()
        {
            var node = SparkSyntax.ParseNode("<div><use content='main'/></div>", position: 6);

            Assert.That(node, Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(((ElementNode)node).Name, Is.EqualTo("use"));
            Assert.That(((ElementNode)node).Attributes.Count, Is.EqualTo(1));
            Assert.That(((ElementNode)node).Attributes[0].Name, Is.EqualTo("content"));
            Assert.That(((ElementNode)node).Attributes[0].Value, Is.EqualTo("main"));
        }

        [Test]
        public void ShouldReturnElementNodeGivenAnUnclosedElementWithValidAttributes()
        {
            var node = SparkSyntax.ParseNode("<div><use content='main' </div>", position: 10);

            Assert.That(node, Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(((ElementNode)node).Name, Is.EqualTo("use"));
            Assert.That(((ElementNode)node).Attributes.Count, Is.EqualTo(1));
            Assert.That(((ElementNode)node).Attributes[0].Name, Is.EqualTo("content"));
            Assert.That(((ElementNode)node).Attributes[0].Value, Is.EqualTo("main"));
        }

        [Test]
        public void ShouldReturnElementNodeGivenAnUnclosedElementAtTheEndOfTheContent()
        {
            var node = SparkSyntax.ParseNode("<use content='main'", position: 10);

            Assert.That(node, Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(((ElementNode)node).Name, Is.EqualTo("use"));
            Assert.That(((ElementNode)node).Attributes.Count, Is.EqualTo(1));
            Assert.That(((ElementNode)node).Attributes[0].Name, Is.EqualTo("content"));
            Assert.That(((ElementNode)node).Attributes[0].Value, Is.EqualTo("main"));
        }

        [Test]
        public void ShouldReturnElementNodeWithoutAttributesWhenGivenAnUnclosedAttributeValue()
        {
            var node = SparkSyntax.ParseNode("<use content='main ", position: 10);

            Assert.That(node, Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(((ElementNode)node).Name, Is.EqualTo("use"));
            Assert.That(((ElementNode)node).Attributes.Count, Is.EqualTo(0));
        }

        [Test]
        public void ShouldReturnTextNodeWhenGivenNewlyOpenedElement()
        {
            var node = SparkSyntax.ParseNode("<div><</div>", position: 6);

            Assert.That(node, Is.InstanceOfType(typeof(TextNode)));
            Assert.That(((TextNode)node).Text, Is.EqualTo("<"));
        }

        [Test]
        public void ShouldReturnTextNodeWhenGivenNewlyOpenedElementAtEndOfLine()
        {
            var node = SparkSyntax.ParseNode(String.Format("<div><{0}</div>", Environment.NewLine), position: 7);

            Assert.That(node, Is.InstanceOfType(typeof(TextNode)));
            Assert.That(((TextNode)node).Text, Is.EqualTo("<"));
        }

        [Test]
        public void ShouldReturnExpressionNodeGivenAClosedExpression()
        {
            var node = SparkSyntax.ParseNode("<div>${item.Text}</div>", position: 7);

            Assert.That(node, Is.InstanceOfType(typeof(ExpressionNode)));
            Assert.That(((ExpressionNode)node).Code.Count, Is.EqualTo(3));
        }

        [Test]
        public void ShouldReturnExpressionNodeGivenAnUnclosedExpression()
        {
            var node = SparkSyntax.ParseNode("<div>${item</div>", position: 11);

            Assert.That(node, Is.InstanceOfType(typeof(ExpressionNode)));
            Assert.That(((ExpressionNode)node).Code.Count, Is.EqualTo(1));
        }
    }
}
