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
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;
using Spark.Tests.Stubs;

namespace Spark.Tests
{
    [TestFixture]
    public class PrefixSupportTester
    {
        private SparkSettings _settings;
        private SparkViewEngine _engine;

        [SetUp]
        public void Init()
        {
            _settings = new SparkSettings()
                .SetPageBaseType(typeof(StubSparkView));

            _engine = new SparkViewEngine(_settings)
            {
                ViewFolder = new FileSystemViewFolder("Spark.Tests.Views")
            };
        }

        static void ContainsInOrder(string content, params string[] values)
        {
            int index = 0;
            foreach (string value in values)
            {
                int nextIndex = content.IndexOf(value, index);
                Assert.GreaterOrEqual(nextIndex, 0, string.Format("Looking for {0}", value));
                index = nextIndex + value.Length;
            }
        }

        [Test]
        public void PrefixFromSettings()
        {
            var settings = new SparkSettings()
                .SetPageBaseType(typeof(StubSparkView))
                .SetPrefix("s");

            var engine = new SparkViewEngine(settings)
                             {
                                 ViewFolder = new FileSystemViewFolder("Spark.Tests.Views")
                             };

            var view = (StubSparkView)engine.CreateInstance(new SparkViewDescriptor().AddTemplate(Path.Combine("Prefix", "prefix-from-settings.spark")));
            view.ViewData["Names"] = new[] { "alpha", "beta", "gamma" };

            var output = new StringWriter();
            view.RenderView(output);

            ContainsInOrder(output.ToString(),
                            "<li", "alpha", "</li>",
                            "<li", "beta", "</li>",
                            "<li", "gamma", "</li>",
                            "<var x=\"1/0\">element ignored</var>",
                            "<p each=\"5\">attribute ignored</p>"
                );
        }


        [Test]
        public void PrefixFromXmlns()
        {
            var view = (StubSparkView)_engine.CreateInstance(new SparkViewDescriptor().AddTemplate(Path.Combine("Prefix", "prefix-from-xmlns.spark")));
            view.ViewData["Names"] = new[] { "alpha", "beta", "gamma" };

            var output = new StringWriter();
            view.RenderView(output);

            ContainsInOrder(output.ToString(),
                            "<li", "alpha", "</li>",
                            "<li", "beta", "</li>",
                            "<li", "gamma", "</li>",
                            "<var x=\"1/0\">element ignored</var>",
                            "<p each=\"5\">attribute ignored</p>");
        }


        [Test]
        public void ConditionalAttributes()
        {
            var view = (StubSparkView)_engine.CreateInstance(new SparkViewDescriptor().AddTemplate(Path.Combine("Prefix", "conditional-attributes.spark")));
            var output = new StringWriter();
            view.RenderView(output);

            ContainsInOrder(output.ToString(),
                            "ok1",
                            "ok2",
                            "ok3");

            Assert.IsFalse(output.ToString().Contains("fail"));
            Assert.IsFalse(output.ToString().Contains("if"));
            Assert.IsFalse(output.ToString().Contains("else"));
            Assert.IsFalse(output.ToString().Contains("condition"));
        }

        [Test]
        public void MacroAndContentPrefixes()
        {
            var view =
                (StubSparkView)
                _engine.CreateInstance(new SparkViewDescriptor().AddTemplate(Path.Combine("Prefix", "macro-content-prefix.spark")));
            var output = new StringWriter();
            view.RenderView(output);

            ContainsInOrder(output.ToString(),
                            "<p>one</p>",
                            "<p>two</p>",
                            "<p>Hello, world!</p>",
                            "<p>three</p>",
                            "<p>four</p>",
                            "<macro:ignored>ignored</macro:ignored>",
                            "<content:ignored>ignored</content:ignored>",
                            "<use:ignored>ignored</use:ignored>",
                            "<render:ignored>ignored</render:ignored>",
                            "<segment:ignored>ignored</segment:ignored>"
                );
        }

        [Test]
        public void SegmentAndRenderPrefixes()
        {
            var view =
                (StubSparkView)
                _engine.CreateInstance(new SparkViewDescriptor().AddTemplate(Path.Combine("Prefix", "segment-render-prefix.spark")));
            var output = new StringWriter();
            view.RenderView(output);

            ContainsInOrder(output.ToString(),
                            "<p>one</p>",
                            "<p>two</p>",
                            "<p>three</p>",
                            "<macro:ignored>ignored</macro:ignored>",
                            "<content:ignored>ignored</content:ignored>",
                            "<use:ignored>ignored</use:ignored>",
                            "<render:ignored>ignored</render:ignored>",
                            "<segment:ignored>ignored</segment:ignored>"
                );
        }

        [Test]
        public void SectionAsSegmentAndRenderPrefixes()
        {
            var settings = new SparkSettings()
                .SetPageBaseType(typeof (StubSparkView))
                .SetParseSectionTagAsSegment(true);

            var engine = new SparkViewEngine(settings)
            {
                ViewFolder = new FileSystemViewFolder("Spark.Tests.Views")
            };

            var view =
                (StubSparkView)
                engine.CreateInstance(new SparkViewDescriptor().AddTemplate(Path.Combine("Prefix", "section-render-prefix.spark")));
            var output = new StringWriter();
            view.RenderView(output);

            ContainsInOrder(output.ToString(),
                            "<p>one</p>",
                            "<p>two</p>",
                            "<p>three</p>",
                            "<macro:ignored>ignored</macro:ignored>",
                            "<content:ignored>ignored</content:ignored>",
                            "<use:ignored>ignored</use:ignored>",
                            "<render:ignored>ignored</render:ignored>",
                            "<section:ignored>ignored</section:ignored>"
                );
        }

        [Test]
        public void MacroAndContentPrefixesFromSettings()
        {
            _engine.Settings = new SparkSettings()
                .SetPageBaseType(typeof(StubSparkView))
                .SetPrefix("s");

            var view =
                (StubSparkView)
                _engine.CreateInstance(new SparkViewDescriptor().AddTemplate(Path.Combine("Prefix", "macro-content-prefix-from-settings.spark")));
            var output = new StringWriter();
            view.RenderView(output);

            ContainsInOrder(output.ToString(),
                            "<p>one</p>",
                            "<p>two</p>",
                            "<p>Hello, world!</p>",
                            "<p>three</p>",
                            "<p>four</p>",
                            "<var x=\"1/0\">ignored</var>"
                );
        }
    }
}
