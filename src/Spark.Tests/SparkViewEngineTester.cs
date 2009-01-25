using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests
{
    [TestFixture]
    public class SparkViewEngineTester
    {
        [SetUp]
        public void Init()
        {
            CompiledViewHolder.Current = new CompiledViewHolder();
        }

        [Test]
        public void CompilingCombinedEntry()
        {
            var settings = new SparkSettings();
            var engine = new SparkViewEngine(settings);
            engine.ViewFolder = new InMemoryViewFolder
                                {
                                    {@"one\two.spark", "hello"},
                                    {@"three\four.spark", "world"}
                                };

            var descriptor = new SparkViewDescriptor()
                .SetTargetNamespace("Hello")
                .AddTemplate(@"one\two.spark")
                .AddTemplate(@"three\four.spark");
            var entry = engine.CreateEntry(descriptor);
            Assert.IsAssignableFrom(typeof(CompositeViewEntry), entry);
            var compositeEntry = (CompositeViewEntry) entry;
            Assert.AreEqual(2, compositeEntry.CompiledEntries.Count);
        }

        [Test]
        public void InstantiatingCombinedView()
        {
            var settings = new SparkSettings();
            var engine = new SparkViewEngine(settings);
            engine.ViewFolder = new InMemoryViewFolder
                                {
                                    {@"one\two.spark", "alpha"},
                                    {@"three\four.spark", "beta<use content='view'/>gamma"}
                                };

            var descriptor = new SparkViewDescriptor()
                .SetTargetNamespace("Hello")
                .AddTemplate(@"one\two.spark")
                .AddTemplate(@"three\four.spark");
            var entry = engine.CreateEntry(descriptor);
            var view = entry.CreateInstance();
            var content = view.RenderView();
            Assert.AreEqual("betaalphagamma", content);
        }
    }
}
