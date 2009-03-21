// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
    public class OmitExtraLinesVisitorTester
    {
        OmitExtraLinesVisitor omit;

        [SetUp]
        public void Init()
        {
            omit = new OmitExtraLinesVisitor(new VisitorContext());
        }

        [Test]
        public void RemoveLinesBeforeSpecialNodes()
        {
            var grammar = new MarkupGrammar();
            var nodes = grammar.Nodes(Source("<p>\r\n  <test if='true'>\r\n    <span>was true</span>\r\n  </test>\r\n</p>"));
            var specialNodeVisitor = new SpecialNodeVisitor(new VisitorContext());
            specialNodeVisitor.Accept(nodes.Value);
            omit.Accept(specialNodeVisitor.Nodes);

            Assert.AreEqual(5, omit.Nodes.Count);
            Assert.IsAssignableFrom(typeof(ElementNode), omit.Nodes[0]);
            Assert.IsAssignableFrom(typeof(TextNode), omit.Nodes[1]);
            Assert.AreEqual("", ((TextNode)omit.Nodes[1]).Text);
            Assert.IsAssignableFrom(typeof(SpecialNode), omit.Nodes[2]);

            var childNodes = ((SpecialNode) omit.Nodes[2]).Body;
            Assert.AreEqual(5, childNodes.Count);
            Assert.IsAssignableFrom(typeof(TextNode), childNodes[0]);
            Assert.AreEqual("\r\n    ", ((TextNode)childNodes[0]).Text);
            Assert.IsAssignableFrom(typeof(TextNode), childNodes[4]);
            Assert.AreEqual("", ((TextNode)childNodes[4]).Text);
        }

        private static Position Source(string text)
        {
            return new Position(new SourceContext(text));
        }
    }
}