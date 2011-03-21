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
using System.Configuration;
using System.IO;
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
        [Test]
        public void CanLoadFromAppConfig()
        {
            var config = (SparkSectionHandler)ConfigurationManager.GetSection("spark");
            Assert.IsTrue(config.Compilation.Debug);
        	Assert.AreEqual(NullBehaviour.Strict, config.Compilation.NullBehaviour);
            Assert.AreEqual(1, config.Compilation.Assemblies.Count);
            Assert.AreEqual(typeof(StubSparkView).FullName, config.Pages.PageBaseType);
            Assert.AreEqual(1, config.Pages.Namespaces.Count);
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
            Assert.AreEqual(3, config.Pages.Namespaces.Count);
            Assert.AreEqual(2, config.Compilation.Assemblies.Count);
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

            Assert.IsTrue(settings.Debug);
        	Assert.AreEqual(NullBehaviour.Lenient, settings.NullBehaviour);
            Assert.AreEqual(3, settings.UseNamespaces.Count());
            Assert.AreEqual(2, settings.UseAssemblies.Count());
        }

        [Test]
        public void UseAssemblyAndNamespaceFromSettings()
        {
            var settings = new SparkSettings()
                .AddNamespace("System.Web")
                .AddAssembly("System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
                .SetPageBaseType(typeof(StubSparkView));

            var views = new InMemoryViewFolder
            {
                {Path.Combine("home", "index.spark"), "<div>${ProcessStatus.Alive}</div>"}
            };

            var engine = new SparkViewEngine(settings) {ViewFolder = views};

            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add(Path.Combine("home","index.spark"));

            var contents = engine.CreateInstance(descriptor).RenderView();
            Assert.AreEqual("<div>Alive</div>", contents);
        }
    }
}