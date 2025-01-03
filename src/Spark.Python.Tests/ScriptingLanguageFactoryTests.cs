// Copyright 2008-2024 Louis DeJardin
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

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Spark.Extensions;
using Spark.Python.Compiler;
using Spark.Tests.Stubs;

namespace Spark.Python.Tests
{
    [TestFixture]
    public class ScriptingLanguageFactoryTests
    {
        private SparkViewEngine _engine;

        [SetUp]
        public void Init()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(StubSparkView));

            var sp = new ServiceCollection()
                .AddSpark(settings)
                .AddSingleton<ISparkLanguageFactory, PythonLanguageFactory>()
                .BuildServiceProvider(); ;

            _engine = (SparkViewEngine)sp.GetService<ISparkViewEngine>();

            IronPython.Hosting.Python.CreateEngine();
        }

        [Test]
        public void CreatePythonViewCompiler()
        {
            var descriptor = new SparkViewDescriptor().SetLanguage(LanguageType.Python);
            var viewCompiler = _engine.LanguageFactory.CreateViewCompiler(_engine, descriptor);

            Assert.That(viewCompiler, Is.AssignableFrom(typeof(PythonViewCompiler)));
        }

        [Test]
        public void CreateEntryForView()
        {
            var descriptor = new SparkViewDescriptor().SetLanguage(LanguageType.Python);
            var entry = _engine.CreateEntry(descriptor);
            var view = entry.CreateInstance();

            Assert.That(view, Is.InstanceOf(typeof(StubSparkView)));
        }
    }
}