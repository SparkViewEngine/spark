using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.Parser.Markup;
using SparkSense.Parsing;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class SparkSyntaxTests
    {
        [Test]
        public void IsSparkNodeShouldReturnSpecialNodeForFullElementAtPositionInsideASpecialNode()
        {
            var node = SparkSyntax.ParseNode("<use content='main'/>", position: 1);

            Node sparkNode;
            var isSparkNode = SparkSyntax.IsSpecialNode(node, out sparkNode);

            Assert.That(isSparkNode);
            Assert.IsNotNull(sparkNode);
            Assert.That(sparkNode, Is.InstanceOfType(typeof(SpecialNode)));
        }

        [Test]
        public void IsSparkNodeShouldReturnSpecialNodeForClosedEmptyElement()
        {
            var node = SparkSyntax.ParseNode("<use />", position: 1);

            Node sparkNode;
            var isSparkNode = SparkSyntax.IsSpecialNode(node, out sparkNode);

            Assert.That(isSparkNode);
            Assert.IsNotNull(sparkNode);
            Assert.That(sparkNode, Is.InstanceOfType(typeof(SpecialNode)));
        }

        [Test]
        public void IsSparkNodeShouldReturnSpecialNodeForUnclosedElement()
        {
            var node = SparkSyntax.ParseNode("<use >", position: 1);

            Node sparkNode;
            var isSparkNode = SparkSyntax.IsSpecialNode(node, out sparkNode);

            Assert.That(isSparkNode);
            Assert.IsNotNull(sparkNode);
            Assert.That(sparkNode, Is.InstanceOfType(typeof(SpecialNode)));
        }

        [Test]
        public void IsSparkNodeShouldReturnElementNodeIfNotSparkSyntax()
        {
            var node = SparkSyntax.ParseNode("<div id='products'/>", position: 1);

            Node elementNode;
            var isSparkNode = SparkSyntax.IsSpecialNode(node, out elementNode);

            Assert.That(!isSparkNode);
            Assert.IsNotNull(elementNode);
            Assert.That(elementNode, Is.InstanceOfType(typeof(ElementNode)));
        }

        [Test]
        public void ShouldResultInAttributeNameContext()
        {
            Assert.IsTrue(SparkSyntax.IsPositionInAttributeName("<div><set x</div>", 11));
            Assert.IsFalse(SparkSyntax.IsPositionInAttributeValue("<div><set x</div>", 11));

            Assert.IsTrue(SparkSyntax.IsPositionInAttributeName("<div><set x='500' y='x + '</div>", 11));
            Assert.IsFalse(SparkSyntax.IsPositionInAttributeValue("<div><set x='500' y='x + '</div>", 11));

            Assert.IsTrue(SparkSyntax.IsPositionInAttributeName("<div><set x='500' y</div>", 19));
            Assert.IsFalse(SparkSyntax.IsPositionInAttributeValue("<div><set x='500' y=</div>", 19));

            Assert.IsTrue(SparkSyntax.IsPositionInAttributeName("<div><set x='500' y='x + '</div>", 19));
            Assert.IsFalse(SparkSyntax.IsPositionInAttributeValue("<div><set x='500' y='x + '</div>", 19));
        }

        [Test]
        public void ShouldResultInAttributeValueContext()
        {
            Assert.IsTrue(SparkSyntax.IsPositionInAttributeValue("<div><set x='500' y='x + '</div>", 16));
            Assert.IsFalse(SparkSyntax.IsPositionInAttributeName("<div><set x='500' y='x + '</div>", 16));

            Assert.IsTrue(SparkSyntax.IsPositionInAttributeValue("<div><set x='500' y='x + '</div>", 25));
            Assert.IsFalse(SparkSyntax.IsPositionInAttributeName("<div><set x='500' y='x + '</div>", 25));

            Assert.IsTrue(SparkSyntax.IsPositionInAttributeValue("<div><set x='500' y='x + ' z=\"</div>", 30));
            Assert.IsFalse(SparkSyntax.IsPositionInAttributeName("<div><set x='500' y='x + ' z=\"</div>", 30));

            Assert.IsTrue(SparkSyntax.IsPositionInAttributeValue("<div><test if='x == 5 '</div>", 22));
            Assert.IsFalse(SparkSyntax.IsPositionInAttributeName("<div><test if='x == 5 '</div>", 22));
        }
    }
}
