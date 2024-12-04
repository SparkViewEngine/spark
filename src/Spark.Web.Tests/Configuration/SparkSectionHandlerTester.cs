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

using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Spark.Extensions;
using Spark.FileSystem;
using Spark.Tests;
using Spark.Tests.Stubs;

namespace Spark.Configuration
{
    [TestFixture]
    public class SparkSectionHandlerTester
    {
        [Test]
        public void CanLoadFromAppConfig()
        {
            var config = (SparkSectionHandler)ConfigurationManager.GetSection("spark");
            Assert.That(config.Compilation.Debug, Is.True);
            Assert.That(config.Compilation.NullBehaviour, Is.EqualTo(NullBehaviour.Strict));
            Assert.That(config.Compilation.Assemblies.Count, Is.EqualTo(1));
            Assert.That(config.Pages.BaseClassTypeName, Is.EqualTo(typeof(StubSparkView).FullName));
            Assert.That(config.Pages.Namespaces.Count, Is.EqualTo(1));
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

            Assert.That(config.Compilation.Debug, Is.True);
            Assert.That(config.Pages.Namespaces.Count, Is.EqualTo(3));
            Assert.That(config.Compilation.Assemblies.Count, Is.EqualTo(2));
        }

        [Test]
        public void CreateSettingsFluentInterface()
        {
            var settings = new SparkSettings()
                .SetDebug(true)
                .SetNullBehaviour(NullBehaviour.Lenient)
                .AddNamespace("System")
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("System.Linq")
                .AddAssembly(typeof(TestAttribute).Assembly)
                .AddAssembly("Spark.Tests");

            Assert.That(settings.Debug, Is.True);
            Assert.That(settings.NullBehaviour, Is.EqualTo(NullBehaviour.Lenient));
            Assert.That(settings.UseNamespaces.Count(), Is.EqualTo(3));
            Assert.That(settings.UseAssemblies.Count(), Is.EqualTo(2));
        }

        [Test]
        public void UseAssemblyAndNamespaceFromSettings()
        {
            var settings = new SparkSettings()
                .AddNamespace("System.Web")
                .AddAssembly("System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
                .SetBaseClassTypeName(typeof(StubSparkView));

            var viewFolder = new InMemoryViewFolder
            {
                { Path.Combine("home", "index.spark"), "<div>${ProcessStatus.Alive}</div>" }
            };

            var sp = new ServiceCollection()
                .AddSpark(settings)
                .AddSingleton<IViewFolder>(viewFolder)
                .BuildServiceProvider();

            var engine = (SparkViewEngine)sp.GetService<ISparkViewEngine>();

            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add(Path.Combine("home", "index.spark"));

            var contents = engine.CreateInstance(descriptor).RenderView();

            Assert.That(contents, Is.EqualTo("<div>Alive</div>"));
        }
    }
}