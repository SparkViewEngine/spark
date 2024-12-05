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
// 
using NUnit.Framework;
using Spark.Compiler.NodeVisitors;
using Spark.Parser;
using Spark.Parser.Markup;

namespace Spark.Tests.Visitors
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
            var visitor = new ConditionalAttributeVisitor(new VisitorContext());
            visitor.Accept(nodes);

            Assert.That(visitor.Nodes, Has.Count.EqualTo(1));
            Assert.That(visitor.Nodes[0], Is.AssignableFrom(typeof(SpecialNode)));

            var ifNode = visitor.Nodes[0] as SpecialNode;
            Assert.That(ifNode, Is.Not.Null);
            Assert.That(ifNode.Element.Name, Is.EqualTo("if"));
        }

        [Test]
        public void DetectUnlessAttribute()
        {
            var grammar = new MarkupGrammar();
            string input = "<div unless=\"true\">hello</div>";
            var nodes = grammar.Nodes(new Position(new SourceContext(input))).Value;
            var visitor = new ConditionalAttributeVisitor(new VisitorContext());
            visitor.Accept(nodes);

            Assert.That(visitor.Nodes, Has.Count.EqualTo(1));
            Assert.That(visitor.Nodes[0], Is.AssignableFrom(typeof(SpecialNode)));

            var unlessNode = visitor.Nodes[0] as SpecialNode;
            Assert.That(unlessNode, Is.Not.Null);
            Assert.That(unlessNode.Element.Name, Is.EqualTo("unless"));
        }

        [Test]
        public void ChainConditionalAttribute()
        {
            var grammar = new MarkupGrammar();

            string input = "<div if=\"false\">hello</div><div elseif=\"true\">world</div><else>that's all</else>";
            var nodes = grammar.Nodes(new Position(new SourceContext(input))).Value;
            var visitor0 = new SpecialNodeVisitor(new VisitorContext());
            visitor0.Accept(nodes);
            var visitor = new ConditionalAttributeVisitor(new VisitorContext());
            visitor.Accept(visitor0.Nodes);

            Assert.That(visitor.Nodes, Has.Count.EqualTo(3));
            Assert.That(visitor.Nodes[0], Is.AssignableFrom(typeof(SpecialNode)));
            Assert.That(visitor.Nodes[1], Is.AssignableFrom(typeof(SpecialNode)));
            Assert.That(visitor.Nodes[2], Is.AssignableFrom(typeof(SpecialNode)));

            var ifNode = (SpecialNode)visitor.Nodes[0];
            Assert.That(ifNode.Element.Name, Is.EqualTo("if"));

            var elseifNode = (SpecialNode)visitor.Nodes[1];
            Assert.That(elseifNode.Element.Name, Is.EqualTo("elseif"));

            var elseNode = (SpecialNode)visitor.Nodes[2];
            Assert.That(elseNode.Element.Name, Is.EqualTo("else"));
        }
    }
}