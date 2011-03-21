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
using System.Text;
using NUnit.Framework;
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;
using Spark.Compiler.NodeVisitors;
using Spark.FileSystem;
using Spark.Parser.Markup;
using System.IO;

namespace Spark.Tests
{
    [TestFixture]
    public class SparkExtensionTester
    {
        private SparkViewEngine engine;

        [SetUp]
        public void Init()
        {
            engine = new SparkViewEngine(new SparkSettings().SetPageBaseType("Spark.Tests.Stubs.StubSparkView"))
                         {ViewFolder = new FileSystemViewFolder("Spark.Tests.Views")};
            engine.ExtensionFactory = new StubExtensionFactory();
        }

        [Test]
        public void TestExtensions()
        {
            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add(Path.Combine("Home", "extensionelements.spark"));
            var entry = engine.CreateEntry(descriptor);
            Assert.That(entry.SourceCode.Contains("//this was a test"));
        }
    }

    internal class StubExtensionFactory : ISparkExtensionFactory
    {
        public ISparkExtension CreateExtension(VisitorContext context, ElementNode node)
        {
            if (node.Name == "unittest")
                return new TestExtension();

            return null;
        }
    }

    internal class TestExtension : ISparkExtension
    {
        public void VisitNode(INodeVisitor visitor, IList<Node> body, IList<Chunk> chunks)
        {
            
        }

        public void VisitChunk(IChunkVisitor visitor, OutputLocation location, IList<Chunk> chunks, StringBuilder output)
        {
            if (location == OutputLocation.UsingNamespace)
                output.AppendLine("//this was a test");
        }
    }
}
