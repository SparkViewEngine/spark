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
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Tests.Precompiled;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Spark.Extensions;
using Spark.Tests;

namespace Spark
{
    [TestFixture]
    public class BatchCompilationTester
    {
        private ISparkViewEngine engine;

        [SetUp]
        public void Init()
        {
            var settings = new SparkSettings()
                .SetBaseClassTypeName(typeof(Tests.Stubs.StubSparkView));

            var sp = new ServiceCollection()
                .AddSpark(settings)
                .AddSingleton<IViewFolder>(
                    new InMemoryViewFolder
                    {
                        { Path.Combine("Home", "Index.spark"), "<p>Hello world</p>" },
                        { Path.Combine("Home", "List.spark"), "<ol><li>one</li><li>two</li></ol>" }
                    })
                .BuildServiceProvider();

            engine = (SparkViewEngine)sp.GetService<ISparkViewEngine>();
        }

        [Test]
        public void CompileMultipleDescriptors()
        {
            var descriptors = new[]
                                  {
                                      new SparkViewDescriptor().AddTemplate(Path.Combine("Home","Index.spark")),
                                      new SparkViewDescriptor().AddTemplate(Path.Combine("Home","List.spark"))
                                  };

            var assembly = engine.BatchCompilation(descriptors);

            var types =
                assembly
                    .GetTypes()
                    .Where(x => x.BaseType == typeof(Tests.Stubs.StubSparkView));

            Assert.That(types.Count(), Is.EqualTo(2));

            var entry0 = engine.GetEntry(descriptors[0]);
            var view0 = entry0.CreateInstance();
            var result0 = view0.RenderView();

            Assert.That(result0, Is.EqualTo("<p>Hello world</p>"));

            var entry1 = engine.GetEntry(descriptors[1]);
            var view1 = entry1.CreateInstance();
            var result1 = view1.RenderView();

            Assert.That(result1, Is.EqualTo("<ol><li>one</li><li>two</li></ol>"));

            Assert.That(view1.GetType().Assembly, Is.SameAs(view0.GetType().Assembly));
        }

        [Test]
        public void DescriptorsAreEqual()
        {
            var descriptor = new SparkViewDescriptor()
                .SetTargetNamespace("Foo")
                .AddTemplate(Path.Combine("Home", "Index.spark"));

            var assembly = engine.BatchCompilation(new[] { descriptor });

            var types =
                assembly
                    .GetTypes()
                    .Where(x => x.BaseType == typeof(Tests.Stubs.StubSparkView))
                    .ToArray();

            Assert.That(types.Count(), Is.EqualTo(1));

            var attribs = types[0].GetCustomAttributes(typeof(SparkViewAttribute), false);
            var sparkViewAttrib = (SparkViewAttribute)attribs[0];

            var key0 = descriptor;
            var key1 = sparkViewAttrib.BuildDescriptor();

            Assert.That(key1, Is.EqualTo(key0));
        }

        [Test]
        public void DescriptorsWithNoTargetNamespace()
        {
            var descriptor = new SparkViewDescriptor().AddTemplate(Path.Combine("Home", "Index.spark"));

            var assembly = engine.BatchCompilation(new[] { descriptor });

            var types =
                assembly
                    .GetTypes()
                    .Where(x => x.BaseType == typeof(Tests.Stubs.StubSparkView))
                    .ToArray();

            Assert.That(types.Count(), Is.EqualTo(1));

            var attribs = types[0].GetCustomAttributes(typeof(SparkViewAttribute), false);
            var sparkViewAttrib = (SparkViewAttribute)attribs[0];

            var key0 = descriptor;
            var key1 = sparkViewAttrib.BuildDescriptor();

            Assert.That(key1, Is.EqualTo(key0));
        }

        [Test]
        public void LoadCompiledViews()
        {
            var descriptors = engine.LoadBatchCompilation(GetType().Assembly);

            Assert.That(descriptors.Count, Is.EqualTo(2));

            var view1 = engine.CreateInstance(
                new SparkViewDescriptor()
                    .SetTargetNamespace("Spark.Tests.Precompiled")
                    .AddTemplate(Path.Combine("Foo", "Bar.spark"))
                    .AddTemplate(Path.Combine("Shared", "Quux.spark")));

            Assert.That(view1.GetType(), Is.EqualTo(typeof(View1)));

            var view2 = engine.CreateInstance(
                new SparkViewDescriptor()
                    .SetTargetNamespace("Spark.Tests.Precompiled")
                    .AddTemplate(Path.Combine("Hello", "World.spark"))
                    .AddTemplate(Path.Combine("Shared", "Default.spark")));

            Assert.That(view2.GetType(), Is.EqualTo(typeof(View2)));
        }

        [Test]
        public void AvoidNotSupportedExceptionForDynamicAssemblies()
        {
            var assemblyName = new AssemblyName
            {
                Name = "DynamicAssembly",
            };
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule", "DynamicAssembly.dll");
            var type = moduleBuilder.DefineType("DynamicType", TypeAttributes.Public).CreateType();
            assemblyBuilder.Save("DynamicAssembly.dll");

            Assert.That(type.Assembly.IsDynamic(), Is.True);
        }
    }
}
