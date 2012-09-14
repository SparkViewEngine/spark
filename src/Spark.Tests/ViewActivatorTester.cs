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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;
using Spark.Tests.Stubs;

namespace Spark.Tests
{
    [TestFixture]
    public class ViewActivatorTester
    {
        class TestView : ISparkView
        {
            public void RenderView(TextWriter writer)
            {
                throw new System.NotImplementedException();
            }

            public Guid GeneratedViewId
            {
                get { throw new System.NotImplementedException(); }
            }

            public bool TryGetViewData(string name, out object value)
            {
                throw new System.NotImplementedException();
            }
        }

        class CustomFactory : IViewActivatorFactory, IViewActivator
        {
            public IViewActivator Register(Type type)
            {
                return this;
            }

            public void Unregister(Type type, IViewActivator activator)
            {

            }

            public ISparkView Activate(Type type)
            {
                return new TestView();
            }

            public void Release(Type type, ISparkView view)
            {
            }
        }

        [Test]
        public void CreateViewInstance()
        {
            var type = typeof(TestView);
            var factory = new DefaultViewActivator();
            var activator = factory.Register(type);
            var view = activator.Activate(type);
            Assert.IsNotNull(view);
            Assert.IsAssignableFrom(typeof(TestView), view);
        }

        [Test]
        public void FastCreateViewInstance()
        {
            var type = typeof(TestView);
            var factory = new FastViewActivator();
            var activator = factory.Register(type);
            var view = activator.Activate(type);
            Assert.IsNotNull(view);
            Assert.IsAssignableFrom(typeof(TestView), view);
        }

        [Test]
        public void CustomViewActivator()
        {
            var engine = new SparkViewEngine(
                new SparkSettings().SetPageBaseType(typeof(StubSparkView)))
                             {
                                 ViewActivatorFactory = new CustomFactory(),
                                 ViewFolder = new InMemoryViewFolder { { "hello/world.spark", "<p>hello world</p>" } }
                             };

            var descriptor = new SparkViewDescriptor().AddTemplate("hello/world.spark");
            var view = engine.CreateInstance(descriptor);

            Assert.IsNotNull(view);
            Assert.IsAssignableFrom(typeof(TestView), view);
        }

        [Test, Explicit]
        public void PerfTest()
        {
            var type = typeof(TestView);
            var defFactory = new DefaultViewActivator();
            var fastFactory = new FastViewActivator();

            var activator = defFactory.Register(type);
            var fastActivator = fastFactory.Register(type);
            var iterations = 1000000;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < iterations; i++)
            {
                var view = activator.Activate(type);
            }
            sw.Stop();
            
            Console.WriteLine("Default took: {0}ms", sw.Elapsed.TotalMilliseconds);
            sw.Reset();

            sw.Start();

            for (int i = 0; i < iterations; i++)
            {
                var view = fastActivator.Activate(type);
            }
            sw.Stop();
            Console.WriteLine("Fast took: {0}ms", sw.Elapsed.TotalMilliseconds);
        }
    }
}
