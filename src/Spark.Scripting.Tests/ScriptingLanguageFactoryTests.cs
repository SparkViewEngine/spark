using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using NUnit.Framework;
using Spark.Scripting.Compiler.Python;
using Spark.Tests.Stubs;

namespace Spark.Scripting.Tests
{
    [TestFixture]
    public class ScriptingLanguageFactoryTests
    {
        private SparkViewEngine _engine;

        [SetUp]
        public void Init()
        {
            _engine = new SparkViewEngine(new SparkSettings())
            {
                LanguageFactory = new ScriptingLanguageFactory(),
                DefaultPageBaseType = typeof(StubSparkView).FullName
            };
            Python.CreateEngine();
        }

        [Test]
        public void CreatePythonViewCompiler()
        {
            var descriptor = new SparkViewDescriptor().SetLanguage(LanguageType.Python);
            var viewCompiler = _engine.LanguageFactory.CreateViewCompiler(_engine, descriptor);
            Assert.IsAssignableFrom(typeof(PythonViewCompiler), viewCompiler);
        }

        [Test]
        public void CreateEntryForView()
        {
            var descriptor = new SparkViewDescriptor().SetLanguage(LanguageType.Python);
            var entry = _engine.CreateEntry(descriptor);
            var view = entry.CreateInstance();
            Assert.IsInstanceOfType(typeof(StubSparkView), view);
        }
    }
}
