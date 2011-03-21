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
using NUnit.Framework;
using Spark.Compiler.NodeVisitors;
using Spark.FileSystem;
using Spark.Parser.Syntax;
using Spark.Tests.Stubs;
using System.IO;

namespace Spark.Tests.Parser
{
    [TestFixture]
    public class CSharpSyntaxProviderTester
    {
        private readonly CSharpSyntaxProvider _syntax = new CSharpSyntaxProvider();

        [Test]
        public void CanParseSimpleFile()
        {
            var context = new VisitorContext { ViewFolder = new FileSystemViewFolder("Spark.Tests.Views") };
            var result = _syntax.GetChunks(context, Path.Combine("Home", "childview.spark"));
            Assert.IsNotNull(result);
        }

        [Test]
        public void UsingCSharpSyntaxInsideEngine()
        {
            // engine takes base class and IViewFolder
            var engine = new SparkViewEngine(
                new SparkSettings().SetPageBaseType("Spark.Tests.Stubs.StubSparkView")) { SyntaxProvider = _syntax, ViewFolder = new FileSystemViewFolder("Spark.Tests.Views") };

            // describe and instantiate view
            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add(Path.Combine("Code", "simplecode.spark"));
            var view = (StubSparkView)engine.CreateInstance(descriptor);

            // provide data and render
            view.ViewData["hello"] = "world";
            var code = view.RenderView();

            Assert.IsNotNull(code);
        }


        [Test]
        public void StatementAndExpressionInCode()
        {
            // engine takes base class and IViewFolder
            var engine = new SparkViewEngine(
                new SparkSettings().SetPageBaseType("Spark.Tests.Stubs.StubSparkView")) { SyntaxProvider = _syntax, ViewFolder = new FileSystemViewFolder("Spark.Tests.Views") };

            // describe and instantiate view
            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add(Path.Combine("Code", "foreach.spark"));
            var view = (StubSparkView)engine.CreateInstance(descriptor);

            // provide data and render
            view.ViewData["hello"] = "world";
            var code = view.RenderView();

            Assert.IsNotNull(code);
        }
    }
}