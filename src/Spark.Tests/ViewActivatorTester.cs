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
    }
}
