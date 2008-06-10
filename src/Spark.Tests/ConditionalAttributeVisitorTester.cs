using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Compiler.NodeVisitors;
using Spark.Parser;
using Spark.Parser.Markup;

namespace Spark.Tests
{
    [TestFixture]
    public class ConditionalAttributeVisitorTester
    {
        [Test]
        public void DetectIfAttribute()
        {
            var grammar = new MarkupGrammar();
            string input = "<div if=\"true\">hello</div>";
            var nodes = grammar.Nodes(new Position(new SourceContext(input))).Value;
            var visitor = new ConditionalAttributeVisitor();
            visitor.Accept(nodes);

            Assert.AreEqual(1, visitor.Nodes.Count);
            Assert.IsAssignableFrom(typeof(SpecialNode), visitor.Nodes[0]);

            var ifNode = visitor.Nodes[0] as SpecialNode;
            Assert.AreEqual("if", ifNode.Element.Name);
        }

        [Test]
        public void ChainConditionalAttribute()
        {
            var grammar = new MarkupGrammar();

            string input = "<div if=\"false\">hello</div><div elseif=\"true\">world</div><else>that's all</else>";
            var nodes = grammar.Nodes(new Position(new SourceContext(input))).Value;
            var visitor0 = new SpecialNodeVisitor(new string[0]);
            visitor0.Accept(nodes);
            var visitor = new ConditionalAttributeVisitor();
            visitor.Accept(visitor0.Nodes);

            Assert.AreEqual(3, visitor.Nodes.Count);
            Assert.IsAssignableFrom(typeof(SpecialNode), visitor.Nodes[0]);
            Assert.IsAssignableFrom(typeof(SpecialNode), visitor.Nodes[1]);
            Assert.IsAssignableFrom(typeof(SpecialNode), visitor.Nodes[2]);

            var ifNode = (SpecialNode)visitor.Nodes[0];
            Assert.AreEqual("if", ifNode.Element.Name);

            var elseifNode = (SpecialNode)visitor.Nodes[1];
            Assert.AreEqual("elseif", elseifNode.Element.Name);

            var elseNode = (SpecialNode)visitor.Nodes[2];
            Assert.AreEqual("else", elseNode.Element.Name);
        }
    }
}
