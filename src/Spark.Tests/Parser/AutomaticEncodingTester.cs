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
using Spark.Parser;
using Spark.Parser.Markup;
using Spark.Tests.Stubs;
using System.IO;

namespace Spark.Tests.Parser
{
    [TestFixture]
    public class AutomaticEncodingTester
    {
        private ISparkViewEngine _engine;
        private InMemoryViewFolder _viewFolder;
        private ISparkViewEntry _entry;
        private SparkSettings _settings;

        [SetUp]
        public void Init()
        {
            Init(false);
        }

        public void Init(bool automaticEncoding)
        {
            _settings = new SparkSettings()
                .SetPageBaseType(typeof(StubSparkView))
                .SetAutomaticEncoding(automaticEncoding);
            var container = new SparkServiceContainer(_settings);

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

        private Position Source(string content)
        {
            return new Position(new SourceContext(content));
        }

        [Test]
        public void DollarSyntaxHasRawContentWhenDisabled()
        {
            var settings = new ParserSettings {AutomaticEncoding = false};
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(Source("${'hello world'}"));

            Assert.AreEqual(1, result.Value.Count);
            Assert.AreEqual("\"hello world\"", (string)((ExpressionNode)result.Value[0]).Code);
            Assert.IsFalse(((ExpressionNode)result.Value[0]).AutomaticEncoding);
        }

        [Test]
        public void BangSyntaxHasRawContentWhenDisabled()
        {
            var settings = new ParserSettings { AutomaticEncoding = false };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(Source("!{'hello world'}"));

            Assert.AreEqual(1, result.Value.Count);
            Assert.AreEqual("\"hello world\"", (string)((ExpressionNode)result.Value[0]).Code);
            Assert.IsFalse(((ExpressionNode)result.Value[0]).AutomaticEncoding);
        }


        [Test]
        public void DollarHasEncodedContentWhenEnabled()
        {
            var settings = new ParserSettings { AutomaticEncoding = true };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(Source("${'hello world'}"));

            Assert.AreEqual(1, result.Value.Count);
            Assert.AreEqual("\"hello world\"", (string)((ExpressionNode)result.Value[0]).Code);
            Assert.IsTrue(((ExpressionNode)result.Value[0]).AutomaticEncoding);
        }


        [Test]
        public void BangSyntaxStillHasRawContentWhenEnabled()
        {
            var settings = new ParserSettings { AutomaticEncoding = true };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(Source("!{'hello world'}"));

            Assert.AreEqual(1, result.Value.Count);
            Assert.AreEqual("\"hello world\"", (string)((ExpressionNode)result.Value[0]).Code);
            Assert.IsFalse(((ExpressionNode)result.Value[0]).AutomaticEncoding);
        }

        [Test]
        public void AutomaticEncodingFalseAlwaysAllowsRawHtml()
        {
            Init(false);
            _viewFolder.Add(Path.Combine("home", "index.spark"), "${'<span>hello</span>'} !{'<span>world</span>'}");
            var content = RenderView(new SparkViewDescriptor().AddTemplate(Path.Combine("home", "index.spark")));
            Assert.AreEqual("<span>hello</span> <span>world</span>", content);
        }

        [Test]
        public void AutomaticEncodingTrueEncodesDollarSyntax()
        {
            Init(true);
            _viewFolder.Add(Path.Combine("home", "index.spark"), "${'<span>hello</span>'} !{'<span>world</span>'}");
            var content = RenderView(new SparkViewDescriptor().AddTemplate(Path.Combine("home", "index.spark")));
            Assert.AreEqual("&lt;span&gt;hello&lt;/span&gt; <span>world</span>", content);
        }

        [Test]
        public void AutomaticEncodingTrueOmitsRedundantEncoding()
        {
            Init(true);
            _viewFolder.Add(Path.Combine("home", "index.spark"), "${H('<span>hello</span>')} !{H('<span>world</span>')}");
            var content = RenderView(new SparkViewDescriptor().AddTemplate(Path.Combine("home", "index.spark")));
            Assert.AreEqual("&lt;span&gt;hello&lt;/span&gt; &lt;span&gt;world&lt;/span&gt;", content);
        }


        [Test]
        public void HashSyntaxForStatementsByDefault()
        {
            var settings = new ParserSettings { AutomaticEncoding = true };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(Source("  #foo  \r\n"));

            Assert.AreEqual(2, result.Value.Count);
            var statement = result.Value.OfType<StatementNode>().Single();
            Assert.That(statement.Code.ToString(), Is.EqualTo("foo  "));
        }

        [Test]
        public void CustomMarkerForStatements()
        {
            var settings = new ParserSettings { AutomaticEncoding = true, StatementMarker="hi" };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(Source("  hibernate  \r\n"));

            Assert.AreEqual(2, result.Value.Count);
            var statement = result.Value.OfType<StatementNode>().Single();
            Assert.That(statement.Code.ToString(), Is.EqualTo("bernate  "));
        }


        [Test]
        public void HashSyntaxIgnoredWhenCustomMarkerProvided()
        {
            var settings = new ParserSettings { AutomaticEncoding = true, StatementMarker = "hi" };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(Source("  #foo  \r\n"));

            Assert.AreEqual(1, result.Value.Count);
            var statement = result.Value.OfType<StatementNode>().Any();
            Assert.That(statement, Is.False);
        }

    }
}
