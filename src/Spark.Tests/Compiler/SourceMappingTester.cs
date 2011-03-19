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
using NUnit.Framework.SyntaxHelpers;
using Spark.FileSystem;
using Spark.Tests.Stubs;
using System.IO;

namespace Spark.Tests.Compiler
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
                .SetPageBaseType(typeof(StubSparkView));
            var container = new SparkServiceContainer(settings);

            _viewFolder = new InMemoryViewFolder();

            container.SetServiceBuilder<IViewFolder>(c => _viewFolder);

            _engine = container.GetService<ISparkViewEngine>();
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

            Assert.AreEqual("<p>Hello world</p>", contents);
            Assert.AreEqual(1, _entry.SourceMappings.Count);
            Assert.AreEqual("\"world\"", _entry.SourceMappings[0].Source.Value);
            Assert.AreEqual(Path.Combine("Home", "Index.spark"), _entry.SourceMappings[0].Source.Begin.SourceContext.FileName);
            Assert.AreEqual(11, _entry.SourceMappings[0].Source.Begin.Offset);

            var resultOffset = _entry.SourceMappings[0].OutputBegin;
            var resultLength = _entry.SourceMappings[0].OutputEnd - _entry.SourceMappings[0].OutputBegin;
            Assert.AreEqual("\"world\"", _entry.SourceCode.Substring(resultOffset, resultLength));
        }

        [Test]
        public void EmbeddedCodeMapped()
        {
            _viewFolder.Add(Path.Combine("Home", "Index.spark"), "<p><%var x = 5;%>${x}</p>");

            var contents = RenderView(new SparkViewDescriptor()
                                          .AddTemplate(Path.Combine("Home", "Index.spark")));

            Assert.AreEqual("<p>5</p>", contents);
            Assert.AreEqual(2, _entry.SourceMappings.Count);
            Assert.AreEqual("var x = 5;", _entry.SourceMappings[0].Source.Value);
            Assert.AreEqual(Path.Combine("Home", "Index.spark"), _entry.SourceMappings[0].Source.Begin.SourceContext.FileName);
            Assert.AreEqual(5, _entry.SourceMappings[0].Source.Begin.Offset);

            var resultOffset = _entry.SourceMappings[0].OutputBegin;
            var resultLength = _entry.SourceMappings[0].OutputEnd - _entry.SourceMappings[0].OutputBegin;
            Assert.AreEqual("var x = 5;", _entry.SourceCode.Substring(resultOffset, resultLength));
        }


        [Test]
        public void ExpressionInAttributeMapped()
        {
            _viewFolder.Add(Path.Combine("Home", "Index.spark"), "<p class='${\"Hello\"}'>World</p>");

            var contents = RenderView(new SparkViewDescriptor()
                                          .AddTemplate(Path.Combine("Home", "Index.spark")));

            Assert.AreEqual("<p class=\"Hello\">World</p>", contents);
            Assert.AreEqual(1, _entry.SourceMappings.Count);
            Assert.AreEqual("\"Hello\"", _entry.SourceMappings[0].Source.Value);
            Assert.AreEqual(Path.Combine("Home", "Index.spark"), _entry.SourceMappings[0].Source.Begin.SourceContext.FileName);
            Assert.AreEqual(12, _entry.SourceMappings[0].Source.Begin.Offset);

            var resultOffset = _entry.SourceMappings[0].OutputBegin;
            var resultLength = _entry.SourceMappings[0].OutputEnd - _entry.SourceMappings[0].OutputBegin;
            Assert.AreEqual("\"Hello\"", _entry.SourceCode.Substring(resultOffset, resultLength));
        }

        [Test]
        public void SingleQuotesAreAvoided()
        {
            _viewFolder.Add(Path.Combine("Home", "Index.spark"), "<p class=\"${'Hello' + 5}\">World</p>");

            var contents = RenderView(new SparkViewDescriptor()
                                          .AddTemplate(Path.Combine("Home", "Index.spark")));

            Assert.AreEqual("<p class=\"Hello5\">World</p>", contents);
            Assert.AreEqual(2, _entry.SourceMappings.Count);
            Assert.AreEqual("Hello", _entry.SourceMappings[0].Source.Value);
            Assert.AreEqual(Path.Combine("Home", "Index.spark"), _entry.SourceMappings[0].Source.Begin.SourceContext.FileName);
            Assert.AreEqual(13, _entry.SourceMappings[0].Source.Begin.Offset);
            
            Assert.AreEqual(" + 5", _entry.SourceMappings[1].Source.Value);
            Assert.AreEqual(Path.Combine("Home", "Index.spark"), _entry.SourceMappings[1].Source.Begin.SourceContext.FileName);
            Assert.AreEqual(19, _entry.SourceMappings[1].Source.Begin.Offset);

            var resultOffset = _entry.SourceMappings[0].OutputBegin;
            var resultLength = _entry.SourceMappings[0].OutputEnd - _entry.SourceMappings[0].OutputBegin;
            Assert.AreEqual("Hello", _entry.SourceCode.Substring(resultOffset, resultLength));

            resultOffset = _entry.SourceMappings[1].OutputBegin;
            resultLength = _entry.SourceMappings[1].OutputEnd - _entry.SourceMappings[1].OutputBegin;
            Assert.AreEqual(" + 5", _entry.SourceCode.Substring(resultOffset, resultLength));
        }

        [Test]
        public void WarningsShouldNotCauseCompilationToFail() {
            _viewFolder.Add(Path.Combine("Home", "Index.spark"), @"
<p>
## warning I am a warning
Hello
</p>");

            var contents = RenderView(new SparkViewDescriptor()
                                          .AddTemplate(Path.Combine("Home", "Index.spark")));

            Assert.That(contents, Text.Contains("Hello"));
            Assert.That(contents, Text.DoesNotContain("warning"));
        }
    }
}