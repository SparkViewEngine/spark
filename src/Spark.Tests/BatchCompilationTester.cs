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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Tests.Precompiled;
using System.IO;

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
                .SetPageBaseType(typeof(Stubs.StubSparkView));

            engine = new SparkViewEngine(settings)
                         {
                             ViewFolder = new InMemoryViewFolder
                                              {
                                                  {Path.Combine("Home","Index.spark"), "<p>Hello world</p>"},
                                                  {Path.Combine("Home","List.spark"), "<ol><li>one</li><li>two</li></ol>"}
                                              }
                         };
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

        [Test]
        public void DescriptorsAreEqual()
        {
            var descriptor = new SparkViewDescriptor()
                .SetTargetNamespace("Foo")
                .AddTemplate(Path.Combine("Home","Index.spark"));

            var assembly = engine.BatchCompilation(new[] { descriptor });

            var types = assembly.GetTypes();
            Assert.AreEqual(1, types.Count());

            var attribs = types[0].GetCustomAttributes(typeof(SparkViewAttribute), false);
            var sparkViewAttrib = (SparkViewAttribute)attribs[0];

            var key0 = descriptor;
            var key1 = sparkViewAttrib.BuildDescriptor();

            Assert.AreEqual(key0, key1);
        }

        [Test]
        public void DescriptorsWithNoTargetNamespace()
        {
            var descriptor = new SparkViewDescriptor().AddTemplate(Path.Combine("Home","Index.spark"));

            var assembly = engine.BatchCompilation(new[] { descriptor });

            var types = assembly.GetTypes();
            Assert.AreEqual(1, types.Count());

            var attribs = types[0].GetCustomAttributes(typeof(SparkViewAttribute), false);
            var sparkViewAttrib = (SparkViewAttribute)attribs[0];

            var key0 = descriptor;
            var key1 = sparkViewAttrib.BuildDescriptor();

            Assert.AreEqual(key0, key1);
        }

        [Test]
        public void LoadCompiledViews()
        {
            var descriptors = engine.LoadBatchCompilation(GetType().Assembly);
            Assert.AreEqual(2, descriptors.Count);

            var view1 = engine.CreateInstance(new SparkViewDescriptor()
                                      .SetTargetNamespace("Spark.Tests.Precompiled")
                                      .AddTemplate(Path.Combine("Foo","Bar.spark"))
                                      .AddTemplate(Path.Combine("Shared","Quux.spark")));
            Assert.AreEqual(typeof(View1), view1.GetType());

            var view2 = engine.CreateInstance(new SparkViewDescriptor()
                                      .SetTargetNamespace("Spark.Tests.Precompiled")
                                      .AddTemplate(Path.Combine("Hello","World.spark"))
                                      .AddTemplate(Path.Combine("Shared","Default.spark")));
            Assert.AreEqual(typeof(View2), view2.GetType());
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
            Assert.IsTrue(type.Assembly.IsDynamic());
        }

        
    }
}
