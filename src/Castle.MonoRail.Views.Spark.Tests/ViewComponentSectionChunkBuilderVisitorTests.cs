using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Compiler;
using Spark.Parser;
using Spark.Parser.Markup;

namespace Castle.MonoRail.Views.Spark.Tests
{
    [TestFixture]
    public class ViewComponentSectionChunkBuilderVisitorTests
    {
        [Test]
        public void CorrectSectionsCreated()
        {
            var grammar = new MarkupGrammar();
            var nodes = grammar.Nodes(new Position(new SourceContext(
                "<foo>1<tr>2<td>3</foo> <bar>4</td>5</tr>6</bar> stuff <baaz>yadda<baaz></baaz><quux><quux/></baaz>")));
            var visitor = new ViewComponentSectionChunkBuilderVisitor();
            visitor.Accept(nodes.Value);
            Assert.AreEqual(3, visitor.Sections.Count);
            
            Assert.AreEqual(1, visitor.Sections["foo"].Count);

            Assert.AreEqual("1<tr>2<td>3", ((SendLiteralChunk)visitor.Sections["foo"][0]).Text);
        }


    }
}
