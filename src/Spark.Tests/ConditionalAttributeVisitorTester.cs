/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

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
