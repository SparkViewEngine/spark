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

namespace Castle.MonoRail.Views.Spark.Tests
{
    using System.Linq;
    using NUnit.Framework;

    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Helpers;
    using Castle.MonoRail.Framework.Test;
    using Castle.MonoRail.Views.Spark.Tests.Stubs;

    using global::Spark;
    using global::Spark.FileSystem;

    [TestFixture]
    public class SparkBatchCompilerTester
    {
        private SparkViewFactory _factory;

        [SetUp]
        public void Init()
        {
            var settings = new SparkSettings()
                .SetBaseClassTypeName(typeof(SparkView));

            var services = new StubMonoRailServices();
            services.AddService(typeof(ISparkSettings), settings);
            services.AddService(typeof(IViewSourceLoader), new FileAssemblyViewSourceLoader("MonoRail.Tests.Views"));
            services.AddService(typeof(IControllerDescriptorProvider), services.ControllerDescriptorProvider);
            _factory = new SparkViewFactory();
            _factory.Service(services);
        }


        [Test]
        public void CompileBatchDescriptor()
        {
            var batch = new SparkBatchDescriptor();

            batch
                .For<StubController>().Layout("default").Include("Index").Include("List.spark")
                .For<StubController>().Layout("ajax").Include("_Widget");


            var assembly = _factory.Precompile(batch);

            Assert.That(assembly, Is.Not.Null);
            Assert.That(assembly.GetTypes().Count(x => x.BaseType == typeof(SparkView)), Is.EqualTo(3));
        }

        [Test]
        public void DefaultEntryBehavior()
        {
            var batch = new SparkBatchDescriptor();

            batch.For<StubController>();

            var descriptors = _factory.CreateDescriptors(batch);

            Assert.That(descriptors.Count, Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(descriptors[0].Templates.Count, Is.EqualTo(2));
                Assert.That(descriptors[1].Templates.Count, Is.EqualTo(2));

                Assert.That(
                    descriptors.Any(
                        d =>
                        d.Templates.Contains(string.Format("Stub{0}Index.spark", Path.DirectorySeparatorChar)) &&
                        d.Templates.Contains(string.Format("Shared{0}default.spark", Path.DirectorySeparatorChar))));
                Assert.That(
                    descriptors.Any(
                        d =>
                        d.Templates.Contains(string.Format("Stub{0}List.spark", Path.DirectorySeparatorChar)) &&
                        d.Templates.Contains(string.Format("Shared{0}default.spark", Path.DirectorySeparatorChar))));
            });
        }

        [Test]
        public void MultipleLayoutFiles()
        {
            var batch = new SparkBatchDescriptor();

            batch
                .For<StubController>().Layout("default").Layout("alternate").Include("Index").Include("List.spark");

            var assembly = _factory.Precompile(batch);

            Assert.That(assembly, Is.Not.Null);
            Assert.That(assembly.GetTypes().Count(x => x.BaseType == typeof(SparkView)), Is.EqualTo(4));
        }

        [Test]
        public void WildcardIncludeRules()
        {
            var batch = new SparkBatchDescriptor();

            batch
                .For<StubController>().Layout("default").Include("*")
                .For<StubController>().Layout("ajax").Include("_*");

            var descriptors = _factory.CreateDescriptors(batch);
            Assert.That(descriptors.Count, Is.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(
                            descriptors.Any(
                                d =>
                                d.Templates.Contains(string.Format("Stub{0}Index.spark", Path.DirectorySeparatorChar)) &&
                                d.Templates.Contains(string.Format("Shared{0}default.spark", Path.DirectorySeparatorChar))));
                Assert.That(
                    descriptors.Any(
                        d =>
                        d.Templates.Contains(string.Format("Stub{0}List.spark", Path.DirectorySeparatorChar)) &&
                        d.Templates.Contains(string.Format("Shared{0}default.spark", Path.DirectorySeparatorChar))));
                Assert.That(
                    descriptors.Any(
                        d =>
                        d.Templates.Contains(string.Format("Stub{0}_Widget.spark", Path.DirectorySeparatorChar)) &&
                        d.Templates.Contains(string.Format("Shared{0}ajax.spark", Path.DirectorySeparatorChar))));
            });

            var assembly = _factory.Precompile(batch);

            Assert.That(assembly, Is.Not.Null);
            Assert.That(assembly.GetTypes().Count(x => x.BaseType == typeof(SparkView)), Is.EqualTo(3));
        }

        [Test]
        public void ExcludeRules()
        {
            var batch = new SparkBatchDescriptor();

            batch.For<StubController>().Include("*").Include("_*").Exclude("In*");

            var descriptors = _factory.CreateDescriptors(batch);

            Assert.That(descriptors.Count, Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(descriptors[0].Templates.Count, Is.EqualTo(2));
                Assert.That(descriptors[1].Templates.Count, Is.EqualTo(2));
                Assert.That(
                    descriptors.Any(
                        d =>
                        d.Templates.Contains(string.Format("Stub{0}_Widget.spark", Path.DirectorySeparatorChar)) &&
                        d.Templates.Contains(string.Format("Shared{0}ajax.spark", Path.DirectorySeparatorChar))));
                Assert.That(
                    descriptors.Any(
                        d =>
                        d.Templates.Contains(string.Format("Stub{0}List.spark", Path.DirectorySeparatorChar)) &&
                        d.Templates.Contains(string.Format("Shared{0}default.spark", Path.DirectorySeparatorChar))));
            });
        }

        [Test]
        public void FileWithoutSparkExtensionAreIgnored()
        {
            var batch = new SparkBatchDescriptor();
            batch.For<StubController>();
            var descriptors = _factory.CreateDescriptors(batch);

            // no templates
            Assert.That(descriptors.SelectMany(d => d.Templates).All(t => !t.Contains("Helper")));
        }

        [Test]
        public void ControllersWithHelpersGenerateAccessors()
        {
            var batch = new SparkBatchDescriptor();
            batch.For<FooController>().Include("index");
            _factory.Engine.ViewFolder = new InMemoryViewFolder { { string.Format("foo{0}index.spark", Path.DirectorySeparatorChar), "<p>foo</p>" } };

            var descriptors = _factory.CreateDescriptors(batch);

            Assert.That(descriptors.Count, Is.EqualTo(1));
            Assert.That(descriptors[0].Accessors.Count, Is.EqualTo(1));
            Assert.That(descriptors[0].Accessors[0].Property, Is.EqualTo(typeof(FooHelper).FullName + " Foo"));
        }
    }

    [Helper(typeof(FooHelper), "Foo")]
    public class FooController : Controller
    {
    }

    public class FooHelper : AbstractHelper
    {
    }
}