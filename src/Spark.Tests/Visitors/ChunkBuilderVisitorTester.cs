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
using System.Collections.Generic;
using System.Linq;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.Parser;
using Spark.Parser.Markup;
using NUnit.Framework;
using Spark.Tests.Visitors;

namespace Spark.Tests.Visitors
{
    [TestFixture]
    public class ChunkBuilderVisitorTester : BaseVisitorTester
    {
        [Test]
        public void MakeLiteralChunk()
        {
            var visitor = new ChunkBuilderVisitor(new VisitorContext());
            visitor.Accept(new Node[]
                               {
                                   new ElementNode("span", new List<AttributeNode>(), false),
                                   new TextNode("hello".ToArray()),
                                   new EntityNode("nbsp"),
                                   new TextNode("world".ToArray()),
                                   new EndElementNode("span")
                               });
            Assert.Multiple(() =>
            {
                Assert.That(visitor.Chunks, Has.Count.EqualTo(1));
                Assert.That(((SendLiteralChunk)visitor.Chunks[0]).Text, Is.EqualTo("<span>hello&nbsp;world</span>"));
            });
        }

        [Test]
        public void SelfClosingElementWithAttributes()
        {
            var elt = new ElementNode("img", new[]
                                                 {
                                                     new AttributeNode("href", '"', new []{new TextNode("urn:picture".ToArray())}),
                                                     new AttributeNode("alt", '"', new Node[]{new TextNode("A Picture".ToArray()), new EntityNode("amp")})
                                                 }, true);
            var visitor = new ChunkBuilderVisitor(new VisitorContext());
            visitor.Accept(elt);
            Assert.Multiple(() =>
            {
                Assert.That(visitor.Chunks, Has.Count.EqualTo(1));
                Assert.That(((SendLiteralChunk)visitor.Chunks[0]).Text, Is.EqualTo("<img href=\"urn:picture\" alt=\"A Picture&amp;\"/>"));
            });
        }

        [TestCase("area")]
        [TestCase("base")]
        [TestCase("br")]
        [TestCase("col")]
        [TestCase("command")]
        [TestCase("embed")]
        [TestCase("hr")]
        [TestCase("img")]
        [TestCase("input")]
        [TestCase("keygen")]
        [TestCase("link")]
        [TestCase("meta")]
        [TestCase("param")]
        [TestCase("source")]
        [TestCase("track")]
        [TestCase("wbr")]
        public void VoidElementsSelfClose(string tagName)
        {
            var elt = new ElementNode(tagName, new AttributeNode[] { }, true);
            var visitor = new ChunkBuilderVisitor(new VisitorContext());
            visitor.Accept(elt);
            Assert.Multiple(() =>
            {
                Assert.That(visitor.Chunks.Count(), Is.EqualTo(1));
                Assert.That(((SendLiteralChunk)visitor.Chunks[0]).Text, Is.EqualTo(string.Format("<{0}/>", tagName)));
            });
        }

        [Test]
        public void NonVoidElementDoesNotSelfClose()
        {
            var elt = new ElementNode("span", new AttributeNode[] { }, true);
            var visitor = new ChunkBuilderVisitor(new VisitorContext());
            visitor.Accept(elt);
            Assert.Multiple(() =>
            {
                Assert.That(visitor.Chunks.Count(), Is.EqualTo(1));
                Assert.That(((SendLiteralChunk)visitor.Chunks[0]).Text, Is.EqualTo("<span></span>"));
            });
        }

        [Test]
        public void WritingDocTypes()
        {
            var justName = new DoctypeNode { Name = "html" };
            var systemName = new DoctypeNode { Name = "html2", ExternalId = new ExternalIdInfo { ExternalIdType = "SYSTEM", SystemId = "my-'system'-id" } };
            var publicName = new DoctypeNode { Name = "html3", ExternalId = new ExternalIdInfo { ExternalIdType = "PUBLIC", PublicId = "my-public-id", SystemId = "my-\"other\"system-id" } };

            var visitor = new ChunkBuilderVisitor(new VisitorContext());
            visitor.Accept(new Node[] { justName, systemName, publicName });
            Assert.Multiple(() =>
            {
                Assert.That(visitor.Chunks, Has.Count.EqualTo(1));
                Assert.That(((SendLiteralChunk)visitor.Chunks[0]).Text, Is.EqualTo("<!DOCTYPE html><!DOCTYPE html2 SYSTEM \"my-'system'-id\"><!DOCTYPE html3 PUBLIC \"my-public-id\" 'my-\"other\"system-id'>"));
            });
        }

        [Test]
        public void RenderPartialContainsChunks()
        {
            var nodes = ParseNodes(
                "<foo>hello</foo>",
                new SpecialNodeVisitor(new VisitorContext { PartialFileNames = new[] { "foo" } }));

            var visitor = new ChunkBuilderVisitor(new VisitorContext());
            visitor.Accept(nodes);
            Assert.That(visitor.Chunks, Has.Count.EqualTo(1));
            var renderPartial = (RenderPartialChunk)((ScopeChunk)visitor.Chunks[0]).Body[0];
            Assert.That(renderPartial.Body, Has.Count.EqualTo(1));
            var literal = (SendLiteralChunk)renderPartial.Body[0];
            Assert.That(literal.Text, Is.EqualTo("hello"));
        }

        [Test]
        public void RenderPartialContainsSegments()
        {
            var nodes = ParseNodes(
                "<foo><segment:two>beta</segment:two><segment:one>alpha</segment:one></foo>",
                new PrefixExpandingVisitor(new VisitorContext()),
                new SpecialNodeVisitor(new VisitorContext { PartialFileNames = new[] { "foo" } }));

            var visitor = new ChunkBuilderVisitor(new VisitorContext());
            visitor.Accept(nodes);
            Assert.That(visitor.Chunks, Has.Count.EqualTo(1));
            var renderPartial = (RenderPartialChunk)((ScopeChunk)visitor.Chunks[0]).Body[0];
            Assert.Multiple(() =>
            {
                Assert.That(renderPartial.Body.Count, Is.EqualTo(0));
                Assert.That(renderPartial.Sections, Has.Count.EqualTo(2));
            });
            Assert.Multiple(() =>
            {
                Assert.That(renderPartial.Sections.ContainsKey("one"));
                Assert.That(renderPartial.Sections.ContainsKey("two"));
            });
            var scope = (ScopeChunk)renderPartial.Sections["one"][0];
            var literal = (SendLiteralChunk)scope.Body[0];
            Assert.That(literal.Text, Is.EqualTo("alpha"));
        }

        [Test]
        public void RenderPartialContainsSectionsAsSegmentsIfAliasedInConfig()
        {
            var nodes = ParseNodes(
                "<foo><section:two>beta</section:two><section:one>alpha</section:one></foo>",
                new PrefixExpandingVisitor(new VisitorContext { ParseSectionTagAsSegment = true }),
                new SpecialNodeVisitor(new VisitorContext
                {
                    PartialFileNames = new[] { "foo" },
                    ParseSectionTagAsSegment = true
                }));

            var visitor = new ChunkBuilderVisitor(new VisitorContext { ParseSectionTagAsSegment = true });
            visitor.Accept(nodes);
            Assert.That(visitor.Chunks, Has.Count.EqualTo(1));
            var renderPartial = (RenderPartialChunk)((ScopeChunk)visitor.Chunks[0]).Body[0];
            Assert.Multiple(() =>
            {
                Assert.That(renderPartial.Body.Count, Is.EqualTo(0));
                Assert.That(renderPartial.Sections, Has.Count.EqualTo(2));
            });
            Assert.Multiple(() =>
            {
                Assert.That(renderPartial.Sections.ContainsKey("one"));
                Assert.That(renderPartial.Sections.ContainsKey("two"));
            });
            var scope = (ScopeChunk)renderPartial.Sections["one"][0];
            var literal = (SendLiteralChunk)scope.Body[0];
            Assert.That(literal.Text, Is.EqualTo("alpha"));
        }
    }
}