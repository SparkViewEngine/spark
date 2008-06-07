using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvcContrib.SparkViewEngine;
using MvcContrib.SparkViewEngine.Parser;
using NUnit.Framework;
using Rhino.Mocks;

namespace MvcContrib.UnitTests.SparkViewEngine
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

        [Test]
        public void LookupNonExistantReturnsNull()
        {
            var key = new CompiledViewHolder.Key { ControllerName = "c", ViewName = "v", MasterName = "m" };
            var entry = holder.Lookup(key);
            Assert.IsNull(entry);
        }

        [Test]
        public void LookupReturnsStoredInstance()
        {
            var key = new CompiledViewHolder.Key { ControllerName = "c", ViewName = "v", MasterName = "m" };
            var entry = new CompiledViewHolder.Entry {Key = key, Loader = new ViewLoader()};
            Assert.IsNull(holder.Lookup(key));
            holder.Store(entry);
            Assert.AreSame(entry, holder.Lookup(key));
        }

        [Test]
        public void VariousKeyEqualities()
        {
            var key1 = new CompiledViewHolder.Key { ControllerName = "c", ViewName = "v", MasterName = "m" };
            var key2 = new CompiledViewHolder.Key { ControllerName = "c", ViewName = "v", MasterName = "m" };

            Assert.AreNotSame(key1, key2);
            Assert.AreEqual(key1, key2);

            var key3 = new CompiledViewHolder.Key { ControllerName = "c", ViewName = "v", MasterName = null };
            Assert.AreNotEqual(key1, key3);
            Assert.AreNotEqual(key2, key3);

            var key4 = new CompiledViewHolder.Key { ControllerName = "c", ViewName = "v", MasterName = "M" };
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

            var key = new CompiledViewHolder.Key { ControllerName = "c", ViewName = "v", MasterName = "m" };
            var entry = new CompiledViewHolder.Entry { Key = key, Loader = loader };
            holder.Store(entry);
            Assert.AreSame(entry, holder.Lookup(key));
            isCurrent = false;
            Assert.IsNull(holder.Lookup(key));

        }
    }
}
