// Copyright 2008-2024 Louis DeJardin
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
using Spark.FileSystem;
using Spark.Tests.Stubs;
using System.IO;
using Spark.Bindings;
using Spark.Parser.Syntax;
using Spark.Tests;

namespace Spark.Compiler
{
    [TestFixture]
    public class SourceMappingTester
    {
        private ISparkViewEngine _engine;
        private InMemoryViewFolder _viewFolder;
        private ISparkViewEntry _entry;

        [SetUp]
        public void Init()
        {
            var settings = new SparkSettings()
                .SetBaseClassTypeName(typeof(StubSparkView));

            var partialProvider = new DefaultPartialProvider();

            _viewFolder = new InMemoryViewFolder();

            var batchCompiler = new RoslynBatchCompiler(new SparkSettings());

            _engine = new SparkViewEngine(
                settings,
                new DefaultSyntaxProvider(settings),
                new DefaultViewActivator(),
                new DefaultLanguageFactory(batchCompiler, settings),
                new CompiledViewHolder(),
                _viewFolder,
                batchCompiler,
                partialProvider,
                new DefaultPartialReferenceProvider(partialProvider),
                new DefaultBindingProvider(),
                null);
        }

        private string RenderView(SparkViewDescriptor descriptor)
        {
            _entry = _engine.CreateEntry(descriptor);
            var view = _entry.CreateInstance();
            var contents = view.RenderView();
            _engine.ReleaseInstance(view);
            return contents;
        }

        [Test]
        public void SimpleExpressionsEntirelyMapped()
        {
            _viewFolder.Add(Path.Combine("Home", "Index.spark"), "<p>Hello ${\"world\"}</p>");

            var contents = RenderView(new SparkViewDescriptor()
                                          .AddTemplate(Path.Combine("Home", "Index.spark")));

            Assert.That(contents, Is.EqualTo("<p>Hello world</p>"));
            Assert.That(_entry.SourceMappings.Count, Is.EqualTo(1));
            Assert.That(_entry.SourceMappings[0].Source.Value, Is.EqualTo("\"world\""));
            Assert.That(_entry.SourceMappings[0].Source.Begin.SourceContext.FileName, Is.EqualTo(Path.Combine("Home", "Index.spark")));
            Assert.That(_entry.SourceMappings[0].Source.Begin.Offset, Is.EqualTo(11));

            var resultOffset = _entry.SourceMappings[0].OutputBegin;
            var resultLength = _entry.SourceMappings[0].OutputEnd - _entry.SourceMappings[0].OutputBegin;
            Assert.That(_entry.SourceCode.Substring(resultOffset, resultLength), Is.EqualTo("\"world\""));
        }

        [Test]
        public void EmbeddedCodeMapped()
        {
            _viewFolder.Add(Path.Combine("Home", "Index.spark"), "<p><%var x = 5;%>${x}</p>");

            var contents = RenderView(new SparkViewDescriptor()
                                          .AddTemplate(Path.Combine("Home", "Index.spark")));

            Assert.That(contents, Is.EqualTo("<p>5</p>"));
            Assert.That(_entry.SourceMappings.Count, Is.EqualTo(2));
            Assert.That(_entry.SourceMappings[0].Source.Value, Is.EqualTo("var x = 5;"));
            Assert.That(_entry.SourceMappings[0].Source.Begin.SourceContext.FileName, Is.EqualTo(Path.Combine("Home", "Index.spark")));
            Assert.That(_entry.SourceMappings[0].Source.Begin.Offset, Is.EqualTo(5));

            var resultOffset = _entry.SourceMappings[0].OutputBegin;
            var resultLength = _entry.SourceMappings[0].OutputEnd - _entry.SourceMappings[0].OutputBegin;
            Assert.That(_entry.SourceCode.Substring(resultOffset, resultLength), Is.EqualTo("var x = 5;"));
        }


        [Test]
        public void ExpressionInAttributeMapped()
        {
            _viewFolder.Add(Path.Combine("Home", "Index.spark"), "<p class='${\"Hello\"}'>World</p>");

            var contents = RenderView(new SparkViewDescriptor()
                                          .AddTemplate(Path.Combine("Home", "Index.spark")));

            Assert.That(contents, Is.EqualTo("<p class='Hello'>World</p>"));
            Assert.That(_entry.SourceMappings.Count, Is.EqualTo(1));
            Assert.That(_entry.SourceMappings[0].Source.Value, Is.EqualTo("\"Hello\""));
            Assert.That(_entry.SourceMappings[0].Source.Begin.SourceContext.FileName, Is.EqualTo(Path.Combine("Home", "Index.spark")));
            Assert.That(_entry.SourceMappings[0].Source.Begin.Offset, Is.EqualTo(12));

            var resultOffset = _entry.SourceMappings[0].OutputBegin;
            var resultLength = _entry.SourceMappings[0].OutputEnd - _entry.SourceMappings[0].OutputBegin;
            Assert.That(_entry.SourceCode.Substring(resultOffset, resultLength), Is.EqualTo("\"Hello\""));
        }

        [Test]
        public void SingleQuotesAreAvoided()
        {
            _viewFolder.Add(Path.Combine("Home", "Index.spark"), "<p class=\"${'Hello' + 5}\">World</p>");

            var contents = RenderView(new SparkViewDescriptor()
                                          .AddTemplate(Path.Combine("Home", "Index.spark")));

            Assert.That(contents, Is.EqualTo("<p class=\"Hello5\">World</p>"));
            Assert.That(_entry.SourceMappings.Count, Is.EqualTo(2));
            Assert.That(_entry.SourceMappings[0].Source.Value, Is.EqualTo("Hello"));
            Assert.That(_entry.SourceMappings[0].Source.Begin.SourceContext.FileName, Is.EqualTo(Path.Combine("Home", "Index.spark")));
            Assert.That(_entry.SourceMappings[0].Source.Begin.Offset, Is.EqualTo(13));

            Assert.That(_entry.SourceMappings[1].Source.Value, Is.EqualTo(" + 5"));
            Assert.That(_entry.SourceMappings[1].Source.Begin.SourceContext.FileName, Is.EqualTo(Path.Combine("Home", "Index.spark")));
            Assert.That(_entry.SourceMappings[1].Source.Begin.Offset, Is.EqualTo(19));

            var resultOffset = _entry.SourceMappings[0].OutputBegin;
            var resultLength = _entry.SourceMappings[0].OutputEnd - _entry.SourceMappings[0].OutputBegin;
            Assert.That(_entry.SourceCode.Substring(resultOffset, resultLength), Is.EqualTo("Hello"));

            resultOffset = _entry.SourceMappings[1].OutputBegin;
            resultLength = _entry.SourceMappings[1].OutputEnd - _entry.SourceMappings[1].OutputBegin;
            Assert.That(_entry.SourceCode.Substring(resultOffset, resultLength), Is.EqualTo(" + 5"));
        }

        [Test]
        public void WarningsShouldNotCauseCompilationToFail()
        {
            _viewFolder.Add(Path.Combine("Home", "Index.spark"), @"
<p>
## warning I am a warning
Hello
</p>");

            var contents = RenderView(new SparkViewDescriptor()
                                          .AddTemplate(Path.Combine("Home", "Index.spark")));

            Assert.That(contents, Does.Contain("Hello"));
            Assert.That(contents, Does.Not.Contains("warning"));
        }
    }
}