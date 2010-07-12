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

    }
}
