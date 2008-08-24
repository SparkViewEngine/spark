using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests
{
    [TestFixture]
    public class BatchCompilationTester
    {
        private ISparkViewEngine engine;

        [SetUp]
        public void Init()
        {
            var settings = new SparkSettings()
                .SetDebug(true)
                .SetPageBaseType(typeof(Stubs.StubSparkView));

            engine = new SparkViewEngine(settings)
                         {
                             ViewFolder = new InMemoryViewFolder
                                              {
                                                  {"Home/Index.spark", "<p>Hello world</p>"},
                                                  {"Home/List.spark", "<ol><li>one</li><li>two</li></ol>"}
                                              }
                         };
        }

        [Test]
        public void CompileMultipleDescriptors()
        {
            var descriptors = new[]
                                  {
                                      new SparkViewDescriptor().AddTemplate("Home/Index.spark"),
                                      new SparkViewDescriptor().AddTemplate("Home/List.spark")
                                  };

            var assembly = engine.BatchCompilation(descriptors);

            var types = assembly.GetTypes();
            Assert.AreEqual(2, types.Count());

            var entry0 = engine.GetEntry(descriptors[0]);
            var view0 = entry0.CreateInstance();
            var result0 = view0.RenderView();
            Assert.AreEqual("<p>Hello world</p>", result0);

            var entry1 = engine.GetEntry(descriptors[1]);
            var view1 = entry1.CreateInstance();
            var result1 = view1.RenderView();
            Assert.AreEqual("<ol><li>one</li><li>two</li></ol>", result1);

            Assert.AreSame(view0.GetType().Assembly, view1.GetType().Assembly);
        }
    }
}
