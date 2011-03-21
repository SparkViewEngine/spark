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
using NUnit.Framework.SyntaxHelpers;
using Spark.Parser;
using NUnit.Framework;
using Rhino.Mocks;
using System.IO;

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
        public void LookupNonExistantReturnsNull()
        {
            var key = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "m"));
            var entry = holder.Lookup(key);
            Assert.IsNull(entry);
        }

        [Test]
        public void LookupReturnsStoredInstance()
        {
            var key = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "m"));
            var entry = new CompiledViewEntry { Descriptor = key, Loader = new ViewLoader() };
            Assert.IsNull(holder.Lookup(key));
            holder.Store(entry);
            Assert.AreSame(entry, holder.Lookup(key));
        }

        [Test]
        public void VariousKeyEqualities()
        {
            var key1 = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "m"));
            var key2 = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "m"));

            Assert.AreNotSame(key1, key2);
            Assert.AreEqual(key1, key2);

            var key3 = BuildKey(Path.Combine("c", "v"));
            Assert.AreNotEqual(key1, key3);
            Assert.AreNotEqual(key2, key3);

            var key4 = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "M"));
            Assert.AreEqual(key1, key4);

            Assert.That(!Equals(key1, null));
            Assert.That(!Equals(null, key1));
        }

        bool isCurrent;

        [Test]
        public void ExpiredEntryReturnsNull()
        {
            var loader = MockRepository.GenerateMock<ViewLoader>();

            isCurrent = true;
            Func<bool> foo = () => isCurrent;
            loader.Stub(x => x.IsCurrent()).Do(foo);

            var key = BuildKey(Path.Combine("c", "v"), Path.Combine("shared", "m"));
            var entry = new CompiledViewEntry { Descriptor = key, Loader = loader };
            holder.Store(entry);
            Assert.AreSame(entry, holder.Lookup(key));
            isCurrent = false;
            Assert.IsNull(holder.Lookup(key));

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