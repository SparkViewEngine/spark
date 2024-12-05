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

        [Test]
        public void UnclosedNodeThrowsCompilerException()
        {
            var nodes = ParseNodes("<div><content name='foo'><br></etc></kontent></div>");
            Assert.AreEqual(6, nodes.Count);

            var visitor = new SpecialNodeVisitor(new VisitorContext { Paint = _paint });
            Assert.That(() => visitor.Accept(nodes), Throws.TypeOf<CompilerException>());
        }

        [Test]
        public void MismatchedSpecialNodeThrowsCompilerException()
        {
            var nodes = ParseNodes("<div><content name='foo'><br></etc></for></div>");
            Assert.AreEqual(6, nodes.Count);

            var visitor = new SpecialNodeVisitor(new VisitorContext { Paint = _paint });
            Assert.That(() => visitor.Accept(nodes), Throws.TypeOf<CompilerException>());
        }

        [Test]
        public void ExtraEndSpecialNodeThrowCompilerException()
        {
            var nodes = ParseNodes("<div><content name='foo'/><br></etc></content></div>");
            Assert.AreEqual(6, nodes.Count);

            var visitor = new SpecialNodeVisitor(new VisitorContext { Paint = _paint });
            Assert.That(() => visitor.Accept(nodes), Throws.TypeOf<CompilerException>());
        }
    }
}
