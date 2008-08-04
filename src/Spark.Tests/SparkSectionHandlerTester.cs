using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Configuration;
using Spark.FileSystem;
using Spark.Tests.Stubs;

namespace Spark.Tests
{
    [TestFixture]
    public class SparkSectionHandlerTester
    {
        [SetUp]
        public void Init()
        {
            CompiledViewHolder.Current = new CompiledViewHolder();
        }

        [Test]
        public void CanLoadFromAppConfig()
        {
            var config = (SparkSectionHandler)ConfigurationManager.GetSection("spark");
            Assert.IsTrue(config.Compilation.Debug);
            Assert.AreEqual(1, config.Compilation.Assemblies.Count);
            Assert.AreEqual(1, config.Compilation.Namespaces.Count);
        }

        [Test]
        public void CreateSectionHandlerFluentInterface()
        {
            var config = new SparkSectionHandler()
                .SetDebug(true)
                .AddNamespace("System")
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("System.Linq")
                .AddAssembly(typeof(TestAttribute).Assembly)
                .AddAssembly("Spark.Tests");

            Assert.IsTrue(config.Compilation.Debug);
            Assert.AreEqual(3, config.Compilation.Namespaces.Count);
            Assert.AreEqual(2, config.Compilation.Assemblies.Count);
        }

        [Test]
        public void CreateSettingsFluentInterface()
        {
            var settings = new SparkSettings()
                .SetDebug(true)
                .AddNamespace("System")
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("System.Linq")
                .AddAssembly(typeof(TestAttribute).Assembly)
                .AddAssembly("Spark.Tests");

            Assert.IsTrue(settings.Debug);
            Assert.AreEqual(3, settings.UseNamespaces.Count);
            Assert.AreEqual(2, settings.UseAssemblies.Count);
        }

        [Test]
        public void UseAssemblyAndNamespaceFromSettings()
        {
            var settings = new SparkSettings()
                .AddNamespace("System.Web")
                .AddAssembly("System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

            var views = new InMemoryViewFolder();
            views.Add("Home\\Index.spark", "<div>${ProcessStatus.Alive}</div>");

            var engine = new SparkViewEngine(settings, views);
            engine.BaseClass = typeof(StubSparkView).FullName;

            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add("home\\index.spark");

            var contents = engine.CreateInstance(descriptor).RenderView();
            Assert.AreEqual("<div>Alive</div>", contents);
        }
    }
}
