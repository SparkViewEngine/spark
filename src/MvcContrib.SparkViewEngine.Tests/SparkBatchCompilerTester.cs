using MvcContrib.SparkViewEngine.Tests.Controllers;
using MvcContrib.ViewFactories;
using NUnit.Framework;
using Spark;
using System.Linq;

namespace MvcContrib.SparkViewEngine.Tests
{
    [TestFixture]
    public class SparkBatchCompilerTester
    {
        private SparkViewFactory _factory;
        [SetUp]
        public void Init()
        {
            var settings = new SparkSettings();

            _factory = new SparkViewFactory(settings) { ViewSourceLoader = new FileSystemViewSourceLoader("AspNetMvc.Tests.Views") };
        }

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
            Assert.That(descriptors.Any(d=>d.Templates.Contains("Stub\\Index.spark") && d.Templates.Contains("Shared\\layout.spark")));
            Assert.That(descriptors.Any(d=>d.Templates.Contains("Stub\\List.spark") && d.Templates.Contains("Shared\\layout.spark")));
            Assert.That(descriptors.Any(d=>d.Templates.Contains("Stub\\_Widget.spark") && d.Templates.Contains("Shared\\ajax.spark")));

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
    }
}
