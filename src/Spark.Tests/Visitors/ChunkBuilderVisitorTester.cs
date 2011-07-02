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
            Assert.AreEqual(1, visitor.Chunks.Count);
            Assert.AreEqual("<span>hello&nbsp;world</span>", ((SendLiteralChunk)visitor.Chunks[0]).Text);
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
            Assert.AreEqual(1, visitor.Chunks.Count);
            Assert.AreEqual("<img href=\"urn:picture\" alt=\"A Picture&amp;\"/>", ((SendLiteralChunk)visitor.Chunks[0]).Text);
        }

        [Test]
        public void WritingDocTypes()
        {
            var justName = new DoctypeNode { Name = "html" };
            var systemName = new DoctypeNode { Name = "html2", ExternalId = new ExternalIdInfo { ExternalIdType = "SYSTEM", SystemId = "my-'system'-id" } };
            var publicName = new DoctypeNode { Name = "html3", ExternalId = new ExternalIdInfo { ExternalIdType = "PUBLIC", PublicId = "my-public-id", SystemId = "my-\"other\"system-id" } };

            var visitor = new ChunkBuilderVisitor(new VisitorContext());
            visitor.Accept(new Node[] { justName, systemName, publicName });
            Assert.AreEqual(1, visitor.Chunks.Count);
            Assert.AreEqual("<!DOCTYPE html><!DOCTYPE html2 SYSTEM \"my-'system'-id\"><!DOCTYPE html3 PUBLIC \"my-public-id\" 'my-\"other\"system-id'>", ((SendLiteralChunk)visitor.Chunks[0]).Text);
        }

        [Test]
        public void RenderPartialContainsChunks()
        {
            var nodes = ParseNodes(
                "<foo>hello</foo>",
                new SpecialNodeVisitor(new VisitorContext { PartialFileNames = new[] { "foo" } }));

            var visitor = new ChunkBuilderVisitor(new VisitorContext());
            visitor.Accept(nodes);
            Assert.AreEqual(1, visitor.Chunks.Count);
            var renderPartial = (RenderPartialChunk)((ScopeChunk)visitor.Chunks[0]).Body[0];
            Assert.AreEqual(1, renderPartial.Body.Count);
            var literal = (SendLiteralChunk)renderPartial.Body[0];
            Assert.AreEqual("hello", literal.Text);
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
            Assert.AreEqual(1, visitor.Chunks.Count);
            var renderPartial = (RenderPartialChunk)((ScopeChunk)visitor.Chunks[0]).Body[0];
            Assert.AreEqual(0, renderPartial.Body.Count);
            Assert.AreEqual(2, renderPartial.Sections.Count);
            Assert.That(renderPartial.Sections.ContainsKey("one"));
            Assert.That(renderPartial.Sections.ContainsKey("two"));
            var scope = (ScopeChunk)renderPartial.Sections["one"][0];
            var literal = (SendLiteralChunk)scope.Body[0];
            Assert.AreEqual("alpha", literal.Text);
        }

        [Test]
        public void RenderPartialContainsSectionsAsSegmentsIfAliasedInConfig()
        {
            var nodes = ParseNodes(
                "<foo><section:two>beta</section:two><section:one>alpha</section:one></foo>",
                new PrefixExpandingVisitor(new VisitorContext {ParseSectionTagAsSegment = true}),
                new SpecialNodeVisitor(new VisitorContext
                                           {
                                               PartialFileNames = new[] { "foo" },
                                               ParseSectionTagAsSegment = true
                                           }));

            var visitor = new ChunkBuilderVisitor(new VisitorContext {ParseSectionTagAsSegment = true});
            visitor.Accept(nodes);
            Assert.AreEqual(1, visitor.Chunks.Count);
            var renderPartial = (RenderPartialChunk)((ScopeChunk)visitor.Chunks[0]).Body[0];
            Assert.AreEqual(0, renderPartial.Body.Count);
            Assert.AreEqual(2, renderPartial.Sections.Count);
            Assert.That(renderPartial.Sections.ContainsKey("one"));
            Assert.That(renderPartial.Sections.ContainsKey("two"));
            var scope = (ScopeChunk)renderPartial.Sections["one"][0];
            var literal = (SendLiteralChunk)scope.Body[0];
            Assert.AreEqual("alpha", literal.Text);
        }
    }
}