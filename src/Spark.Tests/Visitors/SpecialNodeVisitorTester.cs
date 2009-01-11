using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;

namespace Spark.Tests.Visitors
{
    [TestFixture]
    public class SpecialNodeVisitorTester : BaseVisitorTester
    {
        [Test]
        public void WellMatchedNodesWrapContent()
        {
            var nodes = ParseNodes("<div><content name='foo'><br></etc></content></div>");
            Assert.AreEqual(6, nodes.Count);

            var visitor = new SpecialNodeVisitor(new VisitorContext());
            visitor.Accept(nodes);

            Assert.AreEqual(3, visitor.Nodes.Count);
            Assert.IsAssignableFrom(typeof(SpecialNode), visitor.Nodes[1]);

            var specialNode = (SpecialNode)visitor.Nodes[1];
            Assert.AreEqual("content", specialNode.Element.Name);
            Assert.AreEqual(2, specialNode.Body.Count);
            Assert.AreEqual("br", ((ElementNode)specialNode.Body[0]).Name);
            Assert.AreEqual("etc", ((EndElementNode)specialNode.Body[1]).Name);
        }

        [Test, ExpectedException(ExceptionType = typeof(CompilerException))]
        public void UnclosedNodeThrowsCompilerException()
        {
            var nodes = ParseNodes("<div><content name='foo'><br></etc></kontent></div>");
            Assert.AreEqual(6, nodes.Count);

            var visitor = new SpecialNodeVisitor(new VisitorContext { Paint = _paint });
            visitor.Accept(nodes);
        }

        [Test, ExpectedException(ExceptionType = typeof(CompilerException))]
        public void MismatchedSpecialNodeThrowsCompilerException()
        {
            var nodes = ParseNodes("<div><content name='foo'><br></etc></for></div>");
            Assert.AreEqual(6, nodes.Count);

            var visitor = new SpecialNodeVisitor(new VisitorContext { Paint = _paint });
            visitor.Accept(nodes);
        }

        [Test, ExpectedException(ExceptionType = typeof(CompilerException))]
        public void ExtraEndSpecialNodeThrowCompilerException()
        {
            var nodes = ParseNodes("<div><content name='foo'/><br></etc></content></div>");
            Assert.AreEqual(6, nodes.Count);

            var visitor = new SpecialNodeVisitor(new VisitorContext { Paint = _paint });
            visitor.Accept(nodes);
        }
    }
}
