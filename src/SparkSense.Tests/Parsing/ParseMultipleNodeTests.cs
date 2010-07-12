using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;
using SparkSense.Parsing;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class ParseMultipleNodeTests
    {
        [Test]
        public void ShouldParseIntoMultipleNodes()
        {
            var nodes = SparkSyntax.ParseNodes("<div><use content='main'/></div>");
            var visitor = new SpecialNodeVisitor(new VisitorContext());
            visitor.Accept(nodes);

            Assert.That(visitor.Nodes.Count, Is.EqualTo(3));
            Assert.That(visitor.Nodes[0], Is.InstanceOfType(typeof(ElementNode)));
            Assert.That(visitor.Nodes[1], Is.InstanceOfType(typeof(SpecialNode)));
            Assert.That(visitor.Nodes[2], Is.InstanceOfType(typeof(EndElementNode)));
        }
    }
}
