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
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;
using NUnit.Framework;

namespace Spark.Tests
{
    [TestFixture]
    public class ChunkBuilderVisitorTester
    {
        [Test]
        public void MakeLiteralChunk()
        {
            ChunkBuilderVisitor visitor = new ChunkBuilderVisitor();
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
			                                 		new AttributeNode("href", new []{new TextNode("urn:picture".ToArray())}),
			                                 		new AttributeNode("alt", new Node[]{new TextNode("A Picture".ToArray()), new EntityNode("amp")})
			                                 	}, true);
            ChunkBuilderVisitor visitor = new ChunkBuilderVisitor();
            visitor.Accept(elt);
            Assert.AreEqual(1, visitor.Chunks.Count);
            Assert.AreEqual("<img href=\"urn:picture\" alt=\"A Picture&amp;\"/>", ((SendLiteralChunk)visitor.Chunks[0]).Text);
        }

        [Test]
        public void WritingDocTypes()
        {
            DoctypeNode justName = new DoctypeNode() { Name = "html" };
            DoctypeNode systemName = new DoctypeNode() { Name = "html2", ExternalId = new ExternalIdInfo() { ExternalIdType = "SYSTEM", SystemId = "my-'system'-id" } };
            DoctypeNode publicName = new DoctypeNode() { Name = "html3", ExternalId = new ExternalIdInfo() { ExternalIdType = "PUBLIC", PublicId = "my-public-id", SystemId = "my-\"other\"system-id" } };

            ChunkBuilderVisitor visitor = new ChunkBuilderVisitor();
            visitor.Accept(new Node[] { justName, systemName, publicName });
            Assert.AreEqual(1, visitor.Chunks.Count);
            Assert.AreEqual("<!DOCTYPE html><!DOCTYPE html2 SYSTEM \"my-'system'-id\"><!DOCTYPE html3 PUBLIC \"my-public-id\" 'my-\"other\"system-id'>", ((SendLiteralChunk)visitor.Chunks[0]).Text);
        }

    }
}