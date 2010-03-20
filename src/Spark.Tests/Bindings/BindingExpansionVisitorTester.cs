using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.Bindings;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;

namespace Spark.Tests.Bindings
{
    [TestFixture]
    public class BindingExpansionVisitorTester
    {
        private VisitorContext _context;
        private BindingExpansionVisitor _visitor;
        private List<Binding> _bindings;

        [SetUp]
        public void Init()
        {
            _bindings = new List<Binding>();
            _context = new VisitorContext { Bindings = _bindings };
            _visitor = new BindingExpansionVisitor(_context);
        }

        [Test]
        public void CallVisitorToProcessNodes()
        {
            var nodes = new Node[] { new ElementNode("hello", new AttributeNode[0], true) };
            _visitor.Accept(nodes);
            Assert.That(_visitor.Nodes.Count, Is.EqualTo(1));
            Assert.That(_visitor.Nodes[0], Is.SameAs(nodes[0]));
        }

        static Binding CreateBinding(string name, params BindingNode[] nodes)
        {
            return new Binding { ElementName = name, Phrases = new[] { new BindingPhrase { Nodes = nodes } } };
        }

        static IList<Node> CreateElement(string name, params AttributeNode[] nodes)
        {
            return new Node[] { new ElementNode(name, nodes, true) };
        }

        static AttributeNode CreateAttribute(string name, params Node[] nodes)
        {
            return new AttributeNode(name, nodes);
        }

        [Test]
        public void MatchingElementReplacedWithExpression()
        {
            _bindings.Add(CreateBinding("hello", new BindingLiteral("world")));

            var nodes = CreateElement("hello");
            _visitor.Accept(nodes);
            Assert.That(_visitor.Nodes.Count, Is.EqualTo(1));
            Assert.That(_visitor.Nodes[0], Is.Not.SameAs(nodes[0]));
            Assert.That(_visitor.Nodes[0], Is.TypeOf(typeof(ExpressionNode)));
            var expression = (ExpressionNode)_visitor.Nodes[0];
            Assert.That(expression.Code.ToString(), Is.EqualTo("world"));
        }

        [Test]
        public void ReplacementTakesAttributesByNameAndTextBecomesStringLiteral()
        {
            _bindings.Add(CreateBinding("hello", new BindingNameReference("foo") { AssumeStringValue = true }));

            var nodes = CreateElement("hello", CreateAttribute("foo", new TextNode("bar")));
            _visitor.Accept(nodes);
            Assert.That(_visitor.Nodes.Count, Is.EqualTo(1));
            Assert.That(_visitor.Nodes[0], Is.Not.SameAs(nodes[0]));
            Assert.That(_visitor.Nodes[0], Is.TypeOf(typeof(ExpressionNode)));
            var expression = (ExpressionNode)_visitor.Nodes[0];
            Assert.That(expression.Code.ToString(), Is.EqualTo("\"bar\""));
        }


        [Test]
        public void ExpressionsInAttributeBecomeCode()
        {
            _bindings.Add(CreateBinding("hello", new BindingNameReference("foo") { AssumeStringValue = true }));

            var nodes = CreateElement("hello", CreateAttribute("foo", new ExpressionNode("bar")));
            _visitor.Accept(nodes);
            Assert.That(_visitor.Nodes.Count, Is.EqualTo(1));
            Assert.That(_visitor.Nodes[0], Is.Not.SameAs(nodes[0]));
            Assert.That(_visitor.Nodes[0], Is.TypeOf(typeof(ExpressionNode)));
            var expression = (ExpressionNode)_visitor.Nodes[0];
            Assert.That(expression.Code.ToString(), Is.EqualTo("bar"));
        }
    }
}
