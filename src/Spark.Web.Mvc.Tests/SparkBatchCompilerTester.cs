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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Web.Mvc.Extensions;
using Spark.Web.Mvc.Tests.Controllers;

namespace Spark.Web.Mvc.Tests
{
    [TestFixture]
    public class SparkPrecompilerTester
    {
        #region Setup/Teardown

        public static IServiceProvider SetupServiceProvider(Action<ServiceCollection> serviceOverrides = null)
        {
            var services = new ServiceCollection();

            services.AddSpark(new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName));

            if (serviceOverrides != null)
            {
                serviceOverrides.Invoke(services);
            }

            services.AddSingleton<SparkPrecompiler>();

            return services.BuildServiceProvider();
        }

        [SetUp]
        public void Init()
        {
            var serviceProvider = SetupServiceProvider(
                s =>
                {
                    s.AddSingleton<IViewFolder>(new FileSystemViewFolder("AspNetMvc.Tests.Views"));
                    s.AddSingleton<SparkPrecompiler>();
                });

            this._precompiler = serviceProvider.GetService<SparkPrecompiler>();
        }

        #endregion

        private SparkPrecompiler _precompiler;

        [Test]
        public void CompileBatchDescriptor()
        {
            var batch = new SparkBatchDescriptor();

            batch
                .For<StubController>().Layout("layout").Include("Index").Include("List.spark")
                .For<StubController>().Layout("ajax").Include("_Widget");

            var assembly = this._precompiler.Precompile(batch);

            Assert.IsNotNull(assembly);
            Assert.AreEqual(3, assembly.GetTypes().Count(x => x.BaseType == typeof(SparkView)));
        }

        [Test]
        public void CanHandleCSharpV3SyntaxWhenLoadedInAppDomainWithoutConfig()
        {
            var appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = Assembly.GetExecutingAssembly().GetCodeBaseDirectory()
            };

            AppDomain sandbox = null;
            try
            {
                sandbox = AppDomain.CreateDomain("sandbox", null, appDomainSetup);

                var remoteRunner = (PrecompileRunner)sandbox.CreateInstanceAndUnwrap(
                    Assembly.GetExecutingAssembly().FullName,
                    typeof(PrecompileRunner).FullName);

                remoteRunner.Precompile();
            }
            finally
            {
                if (sandbox != null)
                {
                    AppDomain.Unload(sandbox);
                }
            }
        }

        public class PrecompileRunner : MarshalByRefObject
        {
            public void Precompile()
            {
                var serviceProvider = SetupServiceProvider(
                    s =>
                    {
                        // Don't know why but the roslin compiler complains
                        // in CanHandleCSharpV3SyntaxWhenLoadedInAppDomainWithoutConfig test
                        s.AddSingleton<IBatchCompiler, CodeDomBatchCompiler>();
                        s.AddSingleton<IViewFolder>(new FileSystemViewFolder("AspNetMvc.Tests.Views"));
                    });

                var factory = serviceProvider.GetService<SparkViewFactory>();

                var batch = new SparkBatchDescriptor();

                batch.For<FailureController>();

                factory.Precompile(batch);
            }
        }

        [Test]
        public void DefaultMatchingRules()
        {
            var batch = new SparkBatchDescriptor();

            batch.For<StubController>();

            var descriptors = this._precompiler.CreateDescriptors(batch);

            Assert.AreEqual(2, descriptors.Count);
            Assert.AreEqual(1, descriptors[0].Templates.Count);
            Assert.AreEqual(1, descriptors[1].Templates.Count);
            Assert.That(descriptors.Any(d => d.Templates.Contains(string.Format("Stub{0}Index.spark", Path.DirectorySeparatorChar))));
            Assert.That(descriptors.Any(d => d.Templates.Contains(string.Format("Stub{0}List.spark", Path.DirectorySeparatorChar))));
        }

        [Test]
        public void ExcludeRules()
        {
            var batch = new SparkBatchDescriptor();

            batch.For<StubController>().Include("*").Include("_*").Exclude("In*");

            var descriptors = this._precompiler.CreateDescriptors(batch);

            Assert.AreEqual(2, descriptors.Count);
            Assert.AreEqual(1, descriptors[0].Templates.Count);
            Assert.AreEqual(1, descriptors[1].Templates.Count);
            Assert.That(descriptors.Any(d => d.Templates.Contains(string.Format("Stub{0}_Widget.spark", Path.DirectorySeparatorChar))));
            Assert.That(descriptors.Any(d => d.Templates.Contains(string.Format("Stub{0}List.spark", Path.DirectorySeparatorChar))));
        }

        [Test]
        public void MultipleLayoutFiles()
        {
            var batch = new SparkBatchDescriptor();

            batch
                .For<StubController>()
                .Layout("layout")
                .Layout("alternate")
                .Include("Index")
                .Include("List.spark");

            var assembly = this._precompiler.Precompile(batch);

            Assert.IsNotNull(assembly);
            Assert.AreEqual(4, assembly.GetTypes().Count(x => x.BaseType == typeof(SparkView)));
        }

        [Test]
        public void WildcardIncludeRules()
        {
            var batch = new SparkBatchDescriptor();

            batch
                .For<StubController>().Layout("layout").Include("*")
                .For<StubController>().Layout("ajax").Include("_*");

            var descriptors = this._precompiler.CreateDescriptors(batch);
            Assert.AreEqual(3, descriptors.Count);
            Assert.That(
                descriptors.Any(
                    d => d.Templates.Contains(string.Format("Stub{0}Index.spark", Path.DirectorySeparatorChar)) && d.Templates.Contains(string.Format("Shared{0}layout.spark", Path.DirectorySeparatorChar))));
            Assert.That(
                descriptors.Any(
                    d => d.Templates.Contains(string.Format("Stub{0}List.spark", Path.DirectorySeparatorChar)) && d.Templates.Contains(string.Format("Shared{0}layout.spark", Path.DirectorySeparatorChar))));
            Assert.That(
                descriptors.Any(
                    d => d.Templates.Contains(string.Format("Stub{0}_Widget.spark", Path.DirectorySeparatorChar)) && d.Templates.Contains(string.Format("Shared{0}ajax.spark", Path.DirectorySeparatorChar))));

            var assembly = this._precompiler.Precompile(batch);

            Assert.IsNotNull(assembly);
            Assert.AreEqual(3, assembly.GetTypes().Count(x => x.BaseType == typeof(SparkView)));
        }

        [Test]
        public void FileWithoutSparkExtensionAreIgnored()
        {
            var viewFolder = new InMemoryViewFolder
            {
                { string.Format("Stub{0}Index.spark", Path.DirectorySeparatorChar), "<p>index</p>" },
                { string.Format("Stub{0}Helper.cs", Path.DirectorySeparatorChar), "// this is a code file" },
                { string.Format("Layouts{0}Stub.spark", Path.DirectorySeparatorChar), "<p>layout</p><use:view/>" },
            };

            var sp = SetupServiceProvider(
                s =>
                {
                    s.AddSingleton<IViewFolder>(viewFolder);
                });

            this._precompiler = sp.GetService<SparkPrecompiler>();

            var batch = new SparkBatchDescriptor();
            batch.For<StubController>();

            var descriptors = this._precompiler.CreateDescriptors(batch);

            Assert.AreEqual(1, descriptors.Count);
            Assert.AreEqual(2, descriptors[0].Templates.Count);
            Assert.AreEqual(string.Format("Stub{0}Index.spark", Path.DirectorySeparatorChar), descriptors[0].Templates[0]);
            Assert.AreEqual(string.Format("Layouts{0}Stub.spark", Path.DirectorySeparatorChar), descriptors[0].Templates[1]);
        }
    }

    public static class FooExtensions
    {
        public static string FooFor<T>(this SparkView view, Expression<Action<T>> action)
        {
            return $"Foo on lambda expression {action}";
        }
    }

    public static class AssemblyExtensions
    {
        /// <summary>
        /// Get the directory where the assembly is found.
        /// </summary>
        /// <remarks>
        /// This is often useful when using a runner (NCover, NUnit etc.) that 
        /// loads assemblies from a temporary <see cref="AppDomain"/>.
        /// </remarks>
        public static string GetCodeBaseDirectory(this Assembly assembly)
        {
            var codeBaseUriString = assembly.CodeBase;
            var uriBuilder = new UriBuilder(codeBaseUriString);
            var codeBasePath = Uri.UnescapeDataString(uriBuilder.Path);
            
            return Path.GetDirectoryName(codeBasePath);
        }
    }
}