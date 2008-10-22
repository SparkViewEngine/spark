using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Compiler;
using Spark.Compiler.CodeDom;
using Spark.Parser;

namespace Spark.Tests
{
    [TestFixture]
    public class CodeDomViewCompilerTester
    {
        private CodeDomViewCompiler _compiler;

        [SetUp]
        public void Init()
        {
            _compiler = new CodeDomViewCompiler("vb") { BaseClass = typeof(Stubs.StubSparkView).FullName, Debug = true };
        }

        static IEnumerable<IList<Chunk>> Chunks(params Chunk[] chunks)
        {
            return new[] { (IList<Chunk>)chunks };
        }

        [Test]
        public void CodeInheritsBaseClass()
        {
            var chunks = Chunks();

            _compiler.BaseClass = "ThisIsTheBaseClass";
            _compiler.GenerateSourceCode(chunks, chunks);
            Assert.That(_compiler.SourceCode.Contains("Inherits ThisIsTheBaseClass"));
        }

        [Test]
        public void CodeInheritsBaseClassWithTModel()
        {
            var chunks = Chunks(new ViewDataModelChunk { TModel = "ThisIsTheModelClass" });

            _compiler.BaseClass = "ThisIsTheBaseClass";
            _compiler.GenerateSourceCode(chunks, chunks);
            Assert.That(_compiler.SourceCode.Contains("Inherits ThisIsTheBaseClass<ThisIsTheModelClass>"));
        }

        [Test]
        public void UsingNamespaces()
        {
            var chunks = Chunks(new UseNamespaceChunk { Namespace = "AnotherNamespace.ToBe.Used" });

            _compiler.GenerateSourceCode(chunks, chunks);
            Assert.That(_compiler.SourceCode.Contains("Imports AnotherNamespace.ToBe.Used"));
        }

        [Test]
        public void TargetNamespaceWithUsingNamespaces()
        {
            var chunks = Chunks(new UseNamespaceChunk { Namespace = "AnotherNamespace.ToBe.Used" });

            _compiler.Descriptor = new SparkViewDescriptor { TargetNamespace = "TargetNamespace.ForThe.GeneratedView" };
            _compiler.GenerateSourceCode(chunks, chunks);
            Assert.That(_compiler.SourceCode.Contains("Namespace TargetNamespace.ForThe.GeneratedView"));
            Assert.That(_compiler.SourceCode.Contains("Imports AnotherNamespace.ToBe.Used"));
        }

        [Test]
        public void ViewDescriptorAttribute()
        {
            var chunks = Chunks();

            _compiler.Descriptor = new SparkViewDescriptor { TargetNamespace = "TargetNamespace.ForThe.GeneratedView", Templates = new[] { "one", "two", "three" } };
            _compiler.GenerateSourceCode(chunks, chunks);
            Assert.That(_compiler.SourceCode.Contains("SparkViewAttribute"));
            Assert.That(_compiler.SourceCode.Contains("one"));
            Assert.That(_compiler.SourceCode.Contains("two"));
            Assert.That(_compiler.SourceCode.Contains("three"));
        }

        [Test]
        public void ContainsGeneratedViewIdProperty()
        {
            var chunks = Chunks();

            _compiler.GenerateSourceCode(chunks, chunks);
            Assert.That(_compiler.SourceCode.Contains("GeneratedViewId"));
            Assert.That(_compiler.SourceCode.Contains("\"" + _compiler.GeneratedViewId + "\""));
        }

        [Test]
        public void SendingLiteralOutput()
        {
            var chunks = Chunks(new SendLiteralChunk { Text = "Hello World" });

            _compiler.GenerateSourceCode(chunks, chunks);
            Assert.That(_compiler.SourceCode.Contains("Hello World"));
        }

        [Test]
        public void SendingIndentedExpressionOutput()
        {
            var chunks = Chunks(new SendExpressionChunk { Code = "5 + 3", Position = new Position(null, 0, 50, 3, null) });

            _compiler.GenerateSourceCode(chunks, chunks);
            Assert.That(_compiler.SourceCode.Contains("5 + 3"));
        }


        [Test]
        public void RenderingSimpleView()
        {
            var chunks = Chunks(new SendLiteralChunk { Text = "Hello World" });
            _compiler.CompileView(chunks, chunks);
            var view = (ISparkView)Activator.CreateInstance(_compiler.CompiledType);
            var contents = new StringWriter();
            view.RenderView(contents);
            Assert.AreEqual("Hello World", contents.ToString());
        }

        [Test]
        public void CompilingSimpleView()
        {
            var chunks = Chunks(new SendLiteralChunk { Text = "Hello World" });
            _compiler.CompileView(chunks, chunks);
            Assert.That(typeof(Stubs.StubSparkView).IsAssignableFrom(_compiler.CompiledType));
        }

    }
}
