using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Markup;
using Spark.Tests.Stubs;

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
            CompiledViewHolder.Current = new CompiledViewHolder();

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
            _viewFolder.Add("home\\index.spark", "${'<span>hello</span>'} !{'<span>world</span>'}");
            var content = RenderView(new SparkViewDescriptor().AddTemplate("home\\index.spark"));
            Assert.AreEqual("<span>hello</span> <span>world</span>", content);
        }

        [Test]
        public void AutomaticEncodingTrueEncodesDollarSyntax()
        {
            Init(true);
            _viewFolder.Add("home\\index.spark", "${'<span>hello</span>'} !{'<span>world</span>'}");
            var content = RenderView(new SparkViewDescriptor().AddTemplate("home\\index.spark"));
            Assert.AreEqual("&lt;span&gt;hello&lt;/span&gt; <span>world</span>", content);
        }

        [Test]
        public void AutomaticEncodingTrueOmitsRedundantEncoding()
        {
            Init(true);
            _viewFolder.Add("home\\index.spark", "${H('<span>hello</span>')} !{H('<span>world</span>')}");
            var content = RenderView(new SparkViewDescriptor().AddTemplate("home\\index.spark"));
            Assert.AreEqual("&lt;span&gt;hello&lt;/span&gt; &lt;span&gt;world&lt;/span&gt;", content);
        }
    }
}
