using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using SparkSense.Parsing;
using Spark.Parser.Markup;
using Spark.Parser;
using System;
using Spark.Compiler.NodeVisitors;
using System.Collections.Generic;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class SparkSyntaxTests
    {
        [Test]
        public void IsSparkNodeShouldReturnSpecialNodeForFullElementAtPositionOne()
        {
            var node = SparkSyntax.ParseNode("<use content='main'/>", position: 1);

            Node sparkNode;
            var isSparkNode = SparkSyntax.IsSparkNode(node, out sparkNode);

            Assert.That(isSparkNode);
            Assert.IsNotNull(sparkNode);
            Assert.That(sparkNode, Is.InstanceOfType(typeof(SpecialNode)));
        }

        [Test]
        public void IsSparkNodeShouldReturnSpecialNodeForClosedEmptyElement()
        {
            var node = SparkSyntax.ParseNode("<use />", position: 1);

            Node sparkNode;
            var isSparkNode = SparkSyntax.IsSparkNode(node, out sparkNode);

            Assert.That(isSparkNode);
            Assert.IsNotNull(sparkNode);
            Assert.That(sparkNode, Is.InstanceOfType(typeof(SpecialNode)));
        }

        [Test]
        public void IsSparkNodeShouldReturnSpecialNodeForUnclosedElement()
        {
            var node = SparkSyntax.ParseNode("<use >", position: 1);

            Node sparkNode;
            var isSparkNode = SparkSyntax.IsSparkNode(node, out sparkNode);

            Assert.That(isSparkNode);
            Assert.IsNotNull(sparkNode);
            Assert.That(sparkNode, Is.InstanceOfType(typeof(SpecialNode)));
        }

        [Test]
        public void IsSparkNodeShouldReturnElementNodeIfNotSparkSyntax()
        {
            var node = SparkSyntax.ParseNode("<div id='products'/>", position: 1);

            Node elementNode;
            var isSparkNode = SparkSyntax.IsSparkNode(node, out elementNode);

            Assert.That(!isSparkNode);
            Assert.IsNotNull(elementNode);
            Assert.That(elementNode, Is.InstanceOfType(typeof(ElementNode)));
        }

        [Test]
        public void ParseContextShouldReturnAttributeNodeGivenPositionTen()
        {
            var nodeType = SparkSyntax.ParseContext("<div><use content='main'/></div>", position: 10);

            Assert.That(nodeType, Is.EqualTo(typeof(AttributeNode)));
        }

        [Test]
        public void ParseContextShouldReturnElementNodeGivenPositionSix()
        {
            Type nodeType = SparkSyntax.ParseContext("<div><</div>", position: 6);

            Assert.That(nodeType, Is.EqualTo(typeof(ElementNode)));
        }

        [Test]
        public void ParseNodesShouldParseIntoMultipleNodes()
        {
            var nodes = SparkSyntax.ParseNodes("<div><use content='main'/></div>");
            var visitor = new SpecialNodeVisitor(new VisitorContext());
            visitor.Accept(nodes);

            Assert.That(visitor.Nodes.Count, Is.EqualTo(3));
            Assert.That(visitor.Nodes[0], Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(visitor.Nodes[1], Is.InstanceOfType(typeof(SpecialNode)));
            Assert.That(visitor.Nodes[2], Is.InstanceOfType(typeof(EndElementNode)));
        }

        [Test]
        public void ParseNodeShouldReturnElementNodeGivenPositionOne()
        {
            var node = SparkSyntax.ParseNode("<div></div>", position: 1);

            Assert.That(node, Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(((ElementNode)node).Name, Is.EqualTo("div"));
        }

        [Test]
        public void ParseNodeShouldReturnNullGivenPositionAtTheBeginning()
        {
            var node = SparkSyntax.ParseNode("<div></div>", position: 0);

            Assert.That(node, Is.Null);
        }

        [Test]
        public void ParseNodeShouldReturnNullGivenPositionAtTheEnd()
        {
            var node = SparkSyntax.ParseNode("<div></div>", position: 11);

            Assert.That(node, Is.Null);
        }

        [Test]
        public void ParseNodeShouldReturnNullGivenPositionInAnEndElement()
        {
            var node = SparkSyntax.ParseNode("<div></div>", position: 10);

            Assert.That(node, Is.Null);
        }

        [Test]
        public void ParseNodeShouldReturnNullGivenPositionBetweenNodes()
        {
            var node = SparkSyntax.ParseNode("<div><use content='main'/></div>", position: 5);

            Assert.That(node, Is.Null);
        }

        [Test]
        public void ParseNodeShouldReturnElementNodeGivenPositionSix()
        {
            var node = SparkSyntax.ParseNode("<div><use content='main'/></div>", position: 6);

            Assert.That(node, Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(((ElementNode)node).Name, Is.EqualTo("use"));
            Assert.That(((ElementNode)node).Attributes.Count, Is.EqualTo(1));
            Assert.That(((ElementNode)node).Attributes[0].Name, Is.EqualTo("content"));
            Assert.That(((ElementNode)node).Attributes[0].Value, Is.EqualTo("main"));
        }

        [Test]
        public void ParseNodeShouldReturnElementNodeGivenAnUnclosedElementWithValidAttributes()
        {
            var node = SparkSyntax.ParseNode("<div><use content='main' </div>", position: 10);

            Assert.That(node, Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(((ElementNode)node).Name, Is.EqualTo("use"));
            Assert.That(((ElementNode)node).Attributes.Count, Is.EqualTo(1));
            Assert.That(((ElementNode)node).Attributes[0].Name, Is.EqualTo("content"));
            Assert.That(((ElementNode)node).Attributes[0].Value, Is.EqualTo("main"));
        }

        [Test]
        public void ParseNodeShouldReturnElementNodeGivenAnUnclosedElementAtTheEndOfTheContent()
        {
            var node = SparkSyntax.ParseNode("<use content='main'", position: 10);

            Assert.That(node, Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(((ElementNode)node).Name, Is.EqualTo("use"));
            Assert.That(((ElementNode)node).Attributes.Count, Is.EqualTo(1));
            Assert.That(((ElementNode)node).Attributes[0].Name, Is.EqualTo("content"));
            Assert.That(((ElementNode)node).Attributes[0].Value, Is.EqualTo("main"));
        }

        [Test]
        public void ParseNodeShouldReturnElementNodeWithoutAttributesWhenGivenAnUnclosedAttributeValue()
        {
            var node = SparkSyntax.ParseNode("<use content='main ", position: 10);

            Assert.That(node, Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(((ElementNode)node).Name, Is.EqualTo("use"));
            Assert.That(((ElementNode)node).Attributes.Count, Is.EqualTo(0));
        }

    }
}
