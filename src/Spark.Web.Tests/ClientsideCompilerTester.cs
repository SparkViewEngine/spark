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

using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Spark.Extensions;
using Spark.FileSystem;

namespace Spark
{
    [TestFixture]
    public class ClientsideCompilerTester
    {
        private static ServiceProvider CreateServiceProvider(ISparkSettings settings, IViewFolder viewFolder)
        {
            return new ServiceCollection()
                .AddSpark(settings)
                .AddSingleton<IViewFolder>(viewFolder)
                .BuildServiceProvider();
        }

        [Test]
        public void GenerateSimpleTemplate()
        {
            var settings = new SparkSettings();

            var viewFolder = new FileSystemViewFolder("Spark.Tests.Views");

            var sp = CreateServiceProvider(settings, viewFolder);

            var engine = sp.GetService<ISparkViewEngine>();

            var descriptor = new SparkViewDescriptor()
                .SetLanguage(LanguageType.Javascript)
                .AddTemplate(Path.Combine("Clientside","simple.spark"));


            var entry = engine.CreateEntry(descriptor);

            Assert.IsNotNull(entry.SourceCode);
            Assert.IsNotEmpty(entry.SourceCode);
        }

        [Test]
        public void AnonymousTypeBecomesHashLikeObject()
        {
            var settings = new SparkSettings();

            var viewFolder = new FileSystemViewFolder("Spark.Tests.Views");

            var sp = CreateServiceProvider(settings, viewFolder);

            var engine = sp.GetService<ISparkViewEngine>();

            var descriptor = new SparkViewDescriptor()
                .SetLanguage(LanguageType.Javascript)
                .AddTemplate(Path.Combine("Clientside","AnonymousTypeBecomesHashLikeObject.spark"));

            var entry = engine.CreateEntry(descriptor);

            Assert.IsNotNull(entry.SourceCode);
            Assert.IsNotEmpty(entry.SourceCode);

            Assert.That(entry.SourceCode, Does.Contain("x = {foo:\"bar\",quux:5}"));
            Assert.That(entry.SourceCode, Does.Contain("HelloWorld({id:23,data:x})"));
        }
    }
}
