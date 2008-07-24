using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Parser.Syntax;
using Spark.Tests.Stubs;

namespace Spark.Tests
{
    [TestFixture]
    public class CSharpSyntaxProviderTester
    {
        private CSharpSyntaxProvider _syntax = new CSharpSyntaxProvider();

        [Test]
        public void CanParseSimpleFile()
        {
            var result = _syntax.GetChunks("Home\\childview.spark", new FileSystemViewFolder("Views"), null);
            Assert.IsNotNull(result);
        }

        [Test]
        public void UsingCSharpSyntaxInsideEngine()
        {
            // engine takes base class and IViewFolder
            var engine = new SparkViewEngine(
                "Spark.Tests.Stubs.StubSparkView",
                new FileSystemViewFolder("Views"));

            // replace the default grammar
            engine.SyntaxProvider = _syntax;

            // describe and instantiate view
            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add("Code\\simplecode.spark");
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
                "Spark.Tests.Stubs.StubSparkView",
                new FileSystemViewFolder("Views"));

            // replace the default grammar
            engine.SyntaxProvider = _syntax;

            // describe and instantiate view
            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add("Code\\foreach.spark");
            var view = (StubSparkView)engine.CreateInstance(descriptor);

            // provide data and render
            view.ViewData["hello"] = "world";
            var code = view.RenderView();

            Assert.IsNotNull(code);
        }
    }
}
