using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Compiler.ChunkVisitors;
using Spark.Compiler.NodeVisitors;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Syntax;
using Spark.Tests.Visitors;

namespace Spark.Tests.Visitors
{
    [TestFixture]
    public class DetectCodeExpressionTester : BaseVisitorTester
    {
        [Test]
        public void FindLoopParameters()
        {
            var nodes = ParseNodes("<for each='var x in new [] {1,2,3}'>${xIndex}${xIsLast}</for>",
                                   new SpecialNodeVisitor(new VisitorContext()));

            var visitor = new ChunkBuilderVisitor(new VisitorContext());
            visitor.Accept(nodes);

            var expressionVisitor = new DetectCodeExpressionVisitor(null);
            var index = expressionVisitor.Add("xIndex");
            var count = expressionVisitor.Add("xCount");
            var isFirst = expressionVisitor.Add("xIsFirst");
            var isLast = expressionVisitor.Add("xIsLast");
            expressionVisitor.Accept(visitor.Chunks);

            Assert.IsTrue(index.Detected);
            Assert.IsFalse(count.Detected);
            Assert.IsFalse(isFirst.Detected);
            Assert.IsTrue(isLast.Detected);
        }

        [Test]
        public void ParametersInPartial()
        {
            var viewFolder = new InMemoryViewFolder
                                 {
                                     {"home\\index.spark", "<for each='var x in new[]{1,2,3}'><Guts/></for>"},
                                     {"home\\_Guts.spark", "<p>${xIndex}</p>"}
                                 };
            var loader = new ViewLoader { SyntaxProvider = new DefaultSyntaxProvider(), ViewFolder = viewFolder };

            var chunks = loader.Load("home\\index.spark");

            var detectCode = new DetectCodeExpressionVisitor(null);
            var index = detectCode.Add("xIndex");
            var count = detectCode.Add("xCount");
            detectCode.Accept(chunks);

            Assert.IsTrue(index.Detected);
            Assert.IsFalse(count.Detected);
        }


        [Test]
        public void ParametersInCallerBody()
        {
            var viewFolder = new InMemoryViewFolder
                                 {
                                     {"home\\index.spark", "<for each='var x in new[]{1,2,3}'><Guts>${xIndex}</Guts></for>"},
                                     {"home\\_Guts.spark", "<p><render/></p>"}
                                 };
            var loader = new ViewLoader { SyntaxProvider = new DefaultSyntaxProvider(), ViewFolder = viewFolder };

            var chunks = loader.Load("home\\index.spark");

            var detectCode = new DetectCodeExpressionVisitor(null);
            var index = detectCode.Add("xIndex");
            var count = detectCode.Add("xCount");
            detectCode.Accept(chunks);

            Assert.IsTrue(index.Detected);
            Assert.IsFalse(count.Detected);
        }

        [Test]
        public void ParametersInNamedSection()
        {
            var viewFolder = new InMemoryViewFolder
                                 {
                                     {"home\\index.spark", "<for each='var x in new[]{1,2,3}'><Guts><section:foo>${xIndex}</section:foo></Guts></for>"},
                                     {"home\\_Guts.spark", "<p><render:foo/></p>"}
                                 };
            var loader = new ViewLoader { SyntaxProvider = new DefaultSyntaxProvider(), ViewFolder = viewFolder };

            var chunks = loader.Load("home\\index.spark");

            var detectCode = new DetectCodeExpressionVisitor(null);
            var index = detectCode.Add("xIndex");
            var count = detectCode.Add("xCount");
            detectCode.Accept(chunks);

            Assert.IsTrue(index.Detected);
            Assert.IsFalse(count.Detected);
        }

        [Test]
        public void IterationInPartialFile()
        {
            var viewFolder = new InMemoryViewFolder
                                 {
                                     {"home\\index.spark", "<Guts items='new[]{1,2,3}'><section:each>${xIndex}</section:each></Guts>"},
                                     {"home\\_Guts.spark", "<for each='var x in items'><render:each/></for>"}
                                 };
            var loader = new ViewLoader { SyntaxProvider = new DefaultSyntaxProvider(), ViewFolder = viewFolder };

            var chunks = loader.Load("home\\index.spark");

            var detectCode = new DetectCodeExpressionVisitor(null);
            var index = detectCode.Add("xIndex");
            var count = detectCode.Add("xCount");
            detectCode.Accept(chunks);

            Assert.IsTrue(index.Detected);
            Assert.IsFalse(count.Detected);
        }
    }
}