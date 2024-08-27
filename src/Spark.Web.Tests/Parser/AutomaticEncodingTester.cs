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

using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Spark.Extensions;
using Spark.FileSystem;
using Spark.Parser.Markup;
using Spark.Tests;
using Spark.Tests.Stubs;

namespace Spark.Parser
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
            this.Init(false);
        }

        public void Init(bool automaticEncoding)
        {
            this._settings = new SparkSettings()
                .SetBaseClassTypeName(typeof(StubSparkView))
                .SetAutomaticEncoding(automaticEncoding);
            
            this._viewFolder = new InMemoryViewFolder();

            var sp = new ServiceCollection()
                .AddSpark(_settings)
                .AddSingleton<IViewFolder>(_viewFolder)
                .BuildServiceProvider();

            _engine = sp.GetService<ISparkViewEngine>();
        }

        private string RenderView(SparkViewDescriptor descriptor)
        {
            this._entry = this._engine.CreateEntry(descriptor);
            var view = this._entry.CreateInstance();
            var contents = view.RenderView();
            this._engine.ReleaseInstance(view);
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
            var result = grammar.Nodes(this.Source("${'hello world'}"));

            Assert.AreEqual(1, result.Value.Count);
            Assert.AreEqual("\"hello world\"", (string)((ExpressionNode)result.Value[0]).Code);
            Assert.IsFalse(((ExpressionNode)result.Value[0]).AutomaticEncoding);
        }

        [Test]
        public void BangSyntaxHasRawContentWhenDisabled()
        {
            var settings = new ParserSettings { AutomaticEncoding = false };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(this.Source("!{'hello world'}"));

            Assert.AreEqual(1, result.Value.Count);
            Assert.AreEqual("\"hello world\"", (string)((ExpressionNode)result.Value[0]).Code);
            Assert.IsFalse(((ExpressionNode)result.Value[0]).AutomaticEncoding);
        }

        [Test]
        public void DollarHasEncodedContentWhenEnabled()
        {
            var settings = new ParserSettings { AutomaticEncoding = true };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(this.Source("${'hello world'}"));

            Assert.AreEqual(1, result.Value.Count);
            Assert.AreEqual("\"hello world\"", (string)((ExpressionNode)result.Value[0]).Code);
            Assert.IsTrue(((ExpressionNode)result.Value[0]).AutomaticEncoding);
        }

        [Test]
        public void BangSyntaxStillHasRawContentWhenEnabled()
        {
            var settings = new ParserSettings { AutomaticEncoding = true };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(this.Source("!{'hello world'}"));

            Assert.AreEqual(1, result.Value.Count);
            Assert.AreEqual("\"hello world\"", (string)((ExpressionNode)result.Value[0]).Code);
            Assert.IsFalse(((ExpressionNode)result.Value[0]).AutomaticEncoding);
        }

        [Test]
        public void AutomaticEncodingFalseAlwaysAllowsRawHtml()
        {
            this.Init(false);
            this._viewFolder.Add(Path.Combine("home", "index.spark"), "${'<span>hello</span>'} !{'<span>world</span>'}");
            var content = this.RenderView(new SparkViewDescriptor().AddTemplate(Path.Combine("home", "index.spark")));
            Assert.AreEqual("<span>hello</span> <span>world</span>", content);
        }

        [Test]
        public void AutomaticEncodingTrueEncodesDollarSyntax()
        {
            this.Init(true);
            this._viewFolder.Add(Path.Combine("home", "index.spark"), "${'<span>hello</span>'} !{'<span>world</span>'}");
            var content = this.RenderView(new SparkViewDescriptor().AddTemplate(Path.Combine("home", "index.spark")));
            Assert.AreEqual("&lt;span&gt;hello&lt;/span&gt; <span>world</span>", content);
        }

        [Test]
        public void HashSyntaxForStatementsByDefault()
        {
            var settings = new ParserSettings { AutomaticEncoding = true };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(this.Source("  #foo  \r\n"));

            Assert.AreEqual(2, result.Value.Count);
            var statement = result.Value.OfType<StatementNode>().Single();
            Assert.That(statement.Code.ToString(), Is.EqualTo("foo  "));
        }

        [Test]
        public void CustomMarkerForStatements()
        {
            var settings = new ParserSettings { AutomaticEncoding = true, StatementMarker="hi" };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(this.Source("  hibernate  \r\n"));

            Assert.AreEqual(2, result.Value.Count);
            var statement = result.Value.OfType<StatementNode>().Single();
            Assert.That(statement.Code.ToString(), Is.EqualTo("bernate  "));
        }

        [Test]
        public void HashSyntaxIgnoredWhenCustomMarkerProvided()
        {
            var settings = new ParserSettings { AutomaticEncoding = true, StatementMarker = "hi" };
            var grammar = new MarkupGrammar(settings);
            var result = grammar.Nodes(this.Source("  #foo  \r\n"));

            Assert.AreEqual(1, result.Value.Count);
            var statement = result.Value.OfType<StatementNode>().Any();
            Assert.That(statement, Is.False);
        }
    }
}
