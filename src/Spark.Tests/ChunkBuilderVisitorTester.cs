using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvcContrib.SparkViewEngine.Compiler;
using MvcContrib.SparkViewEngine.Compiler.NodeVisitors;
using MvcContrib.SparkViewEngine.Parser.Markup;
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
        
	}
}