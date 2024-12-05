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
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;
using Spark.Tests.Visitors;

namespace Spark.Tests.Visitors
{
    [TestFixture]
    public class NamespaceVisitorTester : BaseVisitorTester
    {
        [Test]
        public void AssignNamespaceToElement()
        {
            var nodes = ParseNodes("<foo xmlns:x='http://sparkviewengine.com/x'><x:bar/></foo>");
            var visitor = new NamespaceVisitor(new VisitorContext());
            visitor.Accept(nodes);
            Assert.Multiple(() =>
            {
                Assert.That(((ElementNode)visitor.Nodes[0]).Namespace, Is.EqualTo(""));
                Assert.That(((ElementNode)visitor.Nodes[1]).Namespace, Is.EqualTo("http://sparkviewengine.com/x"));
            });
        }

        [Test]
        public void AssignNamespaceWithDefaultPrefix()
        {
            var nodes = ParseNodes("<foo><quux:bar/></foo>");
            var visitor = new NamespaceVisitor(new VisitorContext { Prefix = "quux" });
            visitor.Accept(nodes);
            Assert.Multiple(() =>
            {
                Assert.That(((ElementNode)visitor.Nodes[0]).Namespace, Is.EqualTo(""));
                Assert.That(((ElementNode)visitor.Nodes[1]).Namespace, Is.EqualTo("http://sparkviewengine.com/"));
            });
        }

        [Test]
        public void AssignNamespaceToAttributes()
        {
            var nodes = ParseNodes("<foo xmlns:x='http://sparkviewengine.com/x'><bar x:if='false'/></foo>");
            var visitor = new NamespaceVisitor(new VisitorContext());
            visitor.Accept(nodes);
            Assert.Multiple(() =>
            {
                Assert.That(((ElementNode)visitor.Nodes[0]).Namespace, Is.EqualTo(""));
                Assert.That(((ElementNode)visitor.Nodes[1]).Namespace, Is.EqualTo(""));

                Assert.That(((ElementNode)visitor.Nodes[1]).Attributes[0].Namespace, Is.EqualTo("http://sparkviewengine.com/x"));
            });
        }

        [Test]
        public void ElementCanUseXmlnsOnSelf()
        {
            var nodes = ParseNodes("<x:foo y:bar='hello' xmlns:x='http://sparkviewengine.com/x' xmlns:y='http://sparkviewengine.com/y'><quux/></foo>");
            var visitor = new NamespaceVisitor(new VisitorContext());
            visitor.Accept(nodes);
            Assert.Multiple(() =>
            {
                Assert.That(((ElementNode)visitor.Nodes[0]).Namespace, Is.EqualTo("http://sparkviewengine.com/x"));
                Assert.That(((ElementNode)visitor.Nodes[1]).Namespace, Is.EqualTo(""));

                Assert.That(((ElementNode)visitor.Nodes[0]).Attributes[0].Name, Is.EqualTo("y:bar"));
                Assert.That(((ElementNode)visitor.Nodes[0]).Attributes[0].Namespace, Is.EqualTo("http://sparkviewengine.com/y"));
            });

        }

        [Test]
        public void ScopeOfXmlnsIsLimited()
        {
            var nodes = ParseNodes("<x:pre/><x:foo xmlns:x='http://sparkviewengine.com/x'/><x:post/>");
            var visitor = new NamespaceVisitor(new VisitorContext());
            visitor.Accept(nodes);
            Assert.Multiple(() =>
            {
                Assert.That(((ElementNode)visitor.Nodes[0]).Name, Is.EqualTo("x:pre"));
                Assert.That(((ElementNode)visitor.Nodes[0]).Namespace, Is.EqualTo(""));
                Assert.That(((ElementNode)visitor.Nodes[1]).Name, Is.EqualTo("x:foo"));
                Assert.That(((ElementNode)visitor.Nodes[1]).Namespace, Is.EqualTo("http://sparkviewengine.com/x"));
                Assert.That(((ElementNode)visitor.Nodes[2]).Name, Is.EqualTo("x:post"));
                Assert.That(((ElementNode)visitor.Nodes[2]).Namespace, Is.EqualTo(""));
            });

            nodes = ParseNodes("<x:pre/><x:foo xmlns:x='http://sparkviewengine.com/x'><x:bar/></x:foo><x:post/>");
            visitor = new NamespaceVisitor(new VisitorContext());
            visitor.Accept(nodes);
            Assert.That(((ElementNode)visitor.Nodes[0]).Name, Is.EqualTo("x:pre"));
            Assert.That(((ElementNode)visitor.Nodes[0]).Namespace, Is.EqualTo(""));
            Assert.That(((ElementNode)visitor.Nodes[1]).Name, Is.EqualTo("x:foo"));
            Assert.That(((ElementNode)visitor.Nodes[1]).Namespace, Is.EqualTo("http://sparkviewengine.com/x"));
            Assert.That(((ElementNode)visitor.Nodes[2]).Name, Is.EqualTo("x:bar"));
            Assert.That(((ElementNode)visitor.Nodes[2]).Namespace, Is.EqualTo("http://sparkviewengine.com/x"));
            Assert.That(((ElementNode)visitor.Nodes[4]).Name, Is.EqualTo("x:post"));
            Assert.That(((ElementNode)visitor.Nodes[4]).Namespace, Is.EqualTo(""));
        }

        [Test]
        public void NestedElementsDontWreckScope()
        {
            var nodes = ParseNodes("<x:pre/><x:foo xmlns:x='http://sparkviewengine.com/x'><x:foo><x:foo/></x:foo></x:foo><x:post/>");
            var visitor = new NamespaceVisitor(new VisitorContext());
            visitor.Accept(nodes);
            Assert.Multiple(() =>
            {
                Assert.That(((ElementNode)visitor.Nodes[0]).Name, Is.EqualTo("x:pre"));
                Assert.That(((ElementNode)visitor.Nodes[0]).Namespace, Is.EqualTo(""));
                Assert.That(((ElementNode)visitor.Nodes[1]).Name, Is.EqualTo("x:foo"));
                Assert.That(((ElementNode)visitor.Nodes[1]).Namespace, Is.EqualTo("http://sparkviewengine.com/x"));
                Assert.That(((ElementNode)visitor.Nodes[2]).Name, Is.EqualTo("x:foo"));
                Assert.That(((ElementNode)visitor.Nodes[2]).Namespace, Is.EqualTo("http://sparkviewengine.com/x"));
                Assert.That(((ElementNode)visitor.Nodes[3]).Name, Is.EqualTo("x:foo"));
                Assert.That(((ElementNode)visitor.Nodes[3]).Namespace, Is.EqualTo("http://sparkviewengine.com/x"));
                Assert.That(((EndElementNode)visitor.Nodes[4]).Name, Is.EqualTo("x:foo"));
                Assert.That(((EndElementNode)visitor.Nodes[4]).Namespace, Is.EqualTo("http://sparkviewengine.com/x"));
                Assert.That(((EndElementNode)visitor.Nodes[5]).Name, Is.EqualTo("x:foo"));
                Assert.That(((EndElementNode)visitor.Nodes[5]).Namespace, Is.EqualTo("http://sparkviewengine.com/x"));
                Assert.That(((ElementNode)visitor.Nodes[6]).Name, Is.EqualTo("x:post"));
                Assert.That(((ElementNode)visitor.Nodes[6]).Namespace, Is.EqualTo(""));
            });

        }
    }
}
