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
using System.IO;

namespace Spark.Tests.Visitors
{
    [TestFixture]
    public class DetectCodeExpressionTester : BaseVisitorTester
    {
        [Test]
        public void FindLoopParameters()
        {
            var context = new VisitorContext
                              {
                                  SyntaxProvider = new DefaultSyntaxProvider(new SparkSettings())
                              };
            var nodes = ParseNodes("<for each='var x in new [] {1,2,3}'>${xIndex}${xIsLast}</for>",
                                   new SpecialNodeVisitor(context));

            var visitor = new ChunkBuilderVisitor(context);
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
                                     {Path.Combine("home", "index.spark"), "<for each='var x in new[]{1,2,3}'><Guts/></for>"},
                                     {Path.Combine("home", "_Guts.spark"), "<p>${xIndex}</p>"}
                                 };
            var loader = new ViewLoader { SyntaxProvider = new DefaultSyntaxProvider(ParserSettings.DefaultBehavior), ViewFolder = viewFolder };

            var chunks = loader.Load(Path.Combine("home", "index.spark"));

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
                                     {Path.Combine("home", "index.spark"), "<for each='var x in new[]{1,2,3}'><Guts>${xIndex}</Guts></for>"},
                                     {Path.Combine("home", "_Guts.spark"), "<p><render/></p>"}
                                 };
            var loader = new ViewLoader { SyntaxProvider = new DefaultSyntaxProvider(ParserSettings.DefaultBehavior), ViewFolder = viewFolder };

            var chunks = loader.Load(Path.Combine("home", "index.spark"));

            var detectCode = new DetectCodeExpressionVisitor(null);
            var index = detectCode.Add("xIndex");
            var count = detectCode.Add("xCount");
            detectCode.Accept(chunks);

            Assert.IsTrue(index.Detected);
            Assert.IsFalse(count.Detected);
        }

        [Test]
        public void ParametersInNamedSegment()
        {
            var viewFolder = new InMemoryViewFolder
                                 {
                                     {Path.Combine("home", "index.spark"), "<for each='var x in new[]{1,2,3}'><Guts><segment:foo>${xIndex}</segment:foo></Guts></for>"},
                                     {Path.Combine("home", "_Guts.spark"), "<p><render:foo/></p>"}
                                 };
            var loader = new ViewLoader { SyntaxProvider = new DefaultSyntaxProvider(ParserSettings.DefaultBehavior), ViewFolder = viewFolder };

            var chunks = loader.Load(Path.Combine("home", "index.spark"));

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
                 {Path.Combine("home", "index.spark"), "<Guts items='new[]{1,2,3}'><segment:each>${xIndex}</segment:each></Guts>"},
                 {Path.Combine("home", "_Guts.spark"), "<for each='var x in items'><render:each/></for>"}
             };
            var loader = new ViewLoader { SyntaxProvider = new DefaultSyntaxProvider(ParserSettings.DefaultBehavior), ViewFolder = viewFolder };

            var chunks = loader.Load(Path.Combine("home", "index.spark"));

            var detectCode = new DetectCodeExpressionVisitor(null);
            var index = detectCode.Add("xIndex");
            var count = detectCode.Add("xCount");
            detectCode.Accept(chunks);

            Assert.IsTrue(index.Detected);
            Assert.IsFalse(count.Detected);
        }
    }
}