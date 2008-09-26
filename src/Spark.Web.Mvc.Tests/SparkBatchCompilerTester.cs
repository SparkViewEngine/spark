// Copyright 2008 Louis DeJardin - http://whereslou.com
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
using System.Linq;
using NUnit.Framework;
using Spark.FileSystem;
using Spark.Web.Mvc.Tests.Controllers;

namespace Spark.Web.Mvc.Tests
{
    [TestFixture]
    public class SparkBatchCompilerTester
    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
            var settings = new SparkSettings();

            _factory = new SparkViewFactory(settings) { ViewFolder = new FileSystemViewFolder("AspNetMvc.Tests.Views") };
        }

        #endregion

        private SparkViewFactory _factory;

        [Test]
        public void CompileBatchDescriptor()
        {
            var batch = new SparkBatchDescriptor();

            batch
                .For<StubController>().Layout("layout").Include("Index").Include("List.spark")
                .For<StubController>().Layout("ajax").Include("_Widget");


            var assembly = _factory.Precompile(batch);

            Assert.IsNotNull(assembly);
            Assert.AreEqual(3, assembly.GetTypes().Length);
        }


        [Test]
        public void DefaultMatchingRules()
        {
            var batch = new SparkBatchDescriptor();

            batch.For<StubController>();

            var descriptors = _factory.CreateDescriptors(batch);

            Assert.AreEqual(2, descriptors.Count);
            Assert.AreEqual(1, descriptors[0].Templates.Count);
            Assert.AreEqual(1, descriptors[1].Templates.Count);
            Assert.That(descriptors.Any(d => d.Templates.Contains("Stub\\Index.spark")));
            Assert.That(descriptors.Any(d => d.Templates.Contains("Stub\\List.spark")));
        }

        [Test]
        public void ExcludeRules()
        {
            var batch = new SparkBatchDescriptor();

            batch.For<StubController>().Include("*").Include("_*").Exclude("In*");

            var descriptors = _factory.CreateDescriptors(batch);

            Assert.AreEqual(2, descriptors.Count);
            Assert.AreEqual(1, descriptors[0].Templates.Count);
            Assert.AreEqual(1, descriptors[1].Templates.Count);
            Assert.That(descriptors.Any(d => d.Templates.Contains("Stub\\_Widget.spark")));
            Assert.That(descriptors.Any(d => d.Templates.Contains("Stub\\List.spark")));
        }

        [Test]
        public void MultipleLayoutFiles()
        {
            var batch = new SparkBatchDescriptor();

            batch
                .For<StubController>().Layout("layout").Layout("alternate").Include("Index").Include("List.spark");


            var assembly = _factory.Precompile(batch);

            Assert.IsNotNull(assembly);
            Assert.AreEqual(4, assembly.GetTypes().Length);
        }

        [Test]
        public void WildcardIncludeRules()
        {
            var batch = new SparkBatchDescriptor();

            batch
                .For<StubController>().Layout("layout").Include("*")
                .For<StubController>().Layout("ajax").Include("_*");

            var descriptors = _factory.CreateDescriptors(batch);
            Assert.AreEqual(3, descriptors.Count);
            Assert.That(
                descriptors.Any(
                    d => d.Templates.Contains("Stub\\Index.spark") && d.Templates.Contains("Shared\\layout.spark")));
            Assert.That(
                descriptors.Any(
                    d => d.Templates.Contains("Stub\\List.spark") && d.Templates.Contains("Shared\\layout.spark")));
            Assert.That(
                descriptors.Any(
                    d => d.Templates.Contains("Stub\\_Widget.spark") && d.Templates.Contains("Shared\\ajax.spark")));

            var assembly = _factory.Precompile(batch);

            Assert.IsNotNull(assembly);
            Assert.AreEqual(3, assembly.GetTypes().Length);
        }

        [Test]
        public void FileWithoutSparkExtensionAreIgnored()
        {
            _factory.ViewFolder = new InMemoryViewFolder
                                      {
                                          {"Stub\\Index.spark", "<p>index</p>"},
                                          {"Stub\\Helper.cs", "// this is a code file"},
                                          {"Layouts\\Stub.spark", "<p>layout</p><use:view/>"},
                                      };
            var batch = new SparkBatchDescriptor();
            batch.For<StubController>();
            var descriptors = _factory.CreateDescriptors(batch);
            Assert.AreEqual(1, descriptors.Count);
            Assert.AreEqual(2, descriptors[0].Templates.Count);
            Assert.AreEqual("Stub\\Index.spark", descriptors[0].Templates[0]);
            Assert.AreEqual("Layouts\\Stub.spark", descriptors[0].Templates[1]);
        }

    }
}