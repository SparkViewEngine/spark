/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Parser;
using NUnit.Framework;
using Rhino.Mocks;

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

        private CompiledViewHolder.Key BuildKey(string controller, string view, string master)
        {
            return new CompiledViewHolder.Key
            {
                Descriptor = new SparkViewDescriptor
                {
                    ControllerName = controller,
                    ViewName = view,
                    MasterName = master
                }
            };
        }

        [Test]
        public void LookupNonExistantReturnsNull()
        {
            var key = BuildKey("c", "v", "m");
            var entry = holder.Lookup(key);
            Assert.IsNull(entry);
        }

        [Test]
        public void LookupReturnsStoredInstance()
        {
            var key = BuildKey("c", "v", "m");
            var entry = new CompiledViewHolder.Entry { Key = key, Loader = new ViewLoader() };
            Assert.IsNull(holder.Lookup(key));
            holder.Store(entry);
            Assert.AreSame(entry, holder.Lookup(key));
        }

        [Test]
        public void VariousKeyEqualities()
        {
            var key1 = BuildKey("c", "v", "m");
            var key2 = BuildKey("c", "v", "m");

            Assert.AreNotSame(key1, key2);
            Assert.AreEqual(key1, key2);

            var key3 = BuildKey("c", "v", null);
            Assert.AreNotEqual(key1, key3);
            Assert.AreNotEqual(key2, key3);

            var key4 = BuildKey("c", "v", "M");
            Assert.AreEqual(key1, key4);

            Assert.That(!object.Equals(key1, null));
            Assert.That(!object.Equals(null, key1));
        }

        bool isCurrent;

        [Test]
        public void ExpiredEntryReturnsNull()
        {
            var mocks = new MockRepository();
            var loader = mocks.CreateMock<ViewLoader>();

            isCurrent = true;
            Func<bool> foo = delegate { return isCurrent; };
            SetupResult.For(loader.IsCurrent()).Do(foo);

            mocks.ReplayAll();

            var key = BuildKey("c", "v", "m");
            var entry = new CompiledViewHolder.Entry { Key = key, Loader = loader };
            holder.Store(entry);
            Assert.AreSame(entry, holder.Lookup(key));
            isCurrent = false;
            Assert.IsNull(holder.Lookup(key));

        }
    }
}