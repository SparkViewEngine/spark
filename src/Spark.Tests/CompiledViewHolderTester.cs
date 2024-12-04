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
using System.IO;
using Spark.Parser;
using NUnit.Framework;
using Rhino.Mocks;
using Spark.Bindings;
using Spark.FileSystem;
using Spark.Parser.Syntax;

namespace Spark.Tests
{
    [TestFixture]
    public class CompiledViewHolderTester
    {
        private CompiledViewHolder holder;

        [SetUp]
        public void Init()
        {
            holder = new CompiledViewHolder();
        }

        private SparkViewDescriptor BuildKey(params string[] templates)
        {
            return new SparkViewDescriptor
            {
                Templates = templates
            };
        }

        [Test]
        public void LookupNonExistentReturnsNull()
        {
            var key = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "m"));
            var entry = holder.Lookup(key);
            Assert.That(entry, Is.Null);
        }

        [Test]
        public void LookupReturnsStoredInstance()
        {
            var key = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "m"));

            var settings = new SparkSettings();

            var partialProvider = new DefaultPartialProvider();

            var viewLoader = new ViewLoader(
                settings,
                MockRepository.GenerateMock<IViewFolder>(),
                new DefaultPartialProvider(),
                new DefaultPartialReferenceProvider(partialProvider),
                null,
                new DefaultSyntaxProvider(ParserSettings.DefaultBehavior),
                null);

            var entry = new CompiledViewEntry { Descriptor = key, Loader = viewLoader };
            Assert.That(holder.Lookup(key), Is.Null);
            holder.Store(entry);

            Assert.That(holder.Lookup(key), Is.SameAs(entry));
        }

        [Test]
        public void VariousKeyEqualities()
        {
            var key1 = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "m"));
            var key2 = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "m"));

            Assert.That(key2, Is.Not.SameAs(key1));
            Assert.That(key2, Is.EqualTo(key1));

            var key3 = BuildKey(Path.Combine("c", "v"));
            Assert.That(key3, Is.Not.EqualTo(key1));
            Assert.That(key3, Is.Not.EqualTo(key2));

            var key4 = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "M"));
            Assert.Multiple(() =>
            {
                Assert.That(key4, Is.EqualTo(key1));

                Assert.That(key1, Is.Not.Null);
            });
        }

        public class FakeViewLoader : ViewLoader
        {
            public FakeViewLoader() : base(null, null, null, null, null, null, null)
            {
            }

            public bool IsCurrentValue { get; set; }

            public override bool IsCurrent()
            {
                return IsCurrentValue;
            }
        }

        [Test]
        public void ExpiredEntryReturnsNull()
        {
            var loader = new FakeViewLoader
            {
                IsCurrentValue = true
            };

            var key = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "m"));

            var entry = new CompiledViewEntry
            {
                Descriptor = key,
                Loader = loader
            };

            holder.Store(entry);

            Assert.That(holder.Lookup(key), Is.SameAs(entry));

            loader.IsCurrentValue = false;

            Assert.That(holder.Lookup(key), Is.Null);
        }

        [Test]
        public void SparkViewDescriptorTargetNamespaceNullAndEmptyAreEqual()
        {
            var desc1 = new SparkViewDescriptor()
                .AddTemplate("foo.spark")
                .SetTargetNamespace(null);

            var desc2 = new SparkViewDescriptor()
                .AddTemplate("foo.spark")
                .SetTargetNamespace("");

            Assert.That(desc1, Is.EqualTo(desc2));
        }

        [Test]
        public void SparkViewDescriptorLangaugesAreDifferent()
        {
            var desc1 = new SparkViewDescriptor()
                .AddTemplate("foo.spark")
                .SetLanguage(LanguageType.Default);

            var desc2 = new SparkViewDescriptor()
                .AddTemplate("foo.spark")
                .SetLanguage(LanguageType.Javascript);

            Assert.That(desc1, Is.Not.EqualTo(desc2));
        }
    }
}