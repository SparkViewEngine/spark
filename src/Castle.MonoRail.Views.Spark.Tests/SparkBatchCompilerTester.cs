using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.Views.Spark;
using Castle.MonoRail.Views.Spark.Tests.Stubs;
using NUnit.Framework;
using Spark;
using System.Linq;

namespace Castle.MonoRail.Views.Spark.Tests
{
    [TestFixture]
    public class SparkBatchCompilerTester
    {
        private SparkViewFactory _factory;

        [SetUp]
        public void Init()
        {
            var settings = new SparkSettings();

            var services = new StubMonoRailServices();
            services.AddService(typeof(IViewSourceLoader), new FileAssemblyViewSourceLoader("MonoRail.Tests.Views"));
            services.AddService(typeof(ISparkViewEngine), new SparkViewEngine(settings));
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

            Assert.IsNotNull(assembly);
            Assert.AreEqual(3, assembly.GetTypes().Length);
        }

        [Test]
        public void DefaultEntryBehavior()
        {
            var batch = new SparkBatchDescriptor();

            batch.For<StubController>();

            var descriptors = _factory.CreateDescriptors(batch);

            Assert.AreEqual(2, descriptors.Count);
            Assert.AreEqual(2, descriptors[0].Templates.Count);
            Assert.AreEqual(2, descriptors[1].Templates.Count);
            Assert.That(descriptors.Any(d => d.Templates.Contains("Stub\\Index.spark") && d.Templates.Contains("Shared\\default.spark")));
            Assert.That(descriptors.Any(d => d.Templates.Contains("Stub\\List.spark") && d.Templates.Contains("Shared\\default.spark")));
        }

        [Test]
        public void MultipleLayoutFiles()
        {
            var batch = new SparkBatchDescriptor();

            batch
                .For<StubController>().Layout("default").Layout("alternate").Include("Index").Include("List.spark");

            var assembly = _factory.Precompile(batch);

            Assert.IsNotNull(assembly);
            Assert.AreEqual(4, assembly.GetTypes().Length);
        }

        [Test]
        public void WildcardIncludeRules()
        {
            var batch = new SparkBatchDescriptor();

            batch
                .For<StubController>().Layout("default").Include("*")
                .For<StubController>().Layout("ajax").Include("_*");

            var descriptors = _factory.CreateDescriptors(batch);
            Assert.AreEqual(3, descriptors.Count);
            Assert.That(descriptors.Any(d => d.Templates.Contains("Stub\\Index.spark") && d.Templates.Contains("Shared\\default.spark")));
            Assert.That(descriptors.Any(d => d.Templates.Contains("Stub\\List.spark") && d.Templates.Contains("Shared\\default.spark")));
            Assert.That(descriptors.Any(d => d.Templates.Contains("Stub\\_Widget.spark") && d.Templates.Contains("Shared\\ajax.spark")));

            var assembly = _factory.Precompile(batch);

            Assert.IsNotNull(assembly);
            Assert.AreEqual(3, assembly.GetTypes().Length);
        }

        [Test]
        public void ExcludeRules()
        {
            var batch = new SparkBatchDescriptor();

            batch.For<StubController>().Include("*").Include("_*").Exclude("In*");

            var descriptors = _factory.CreateDescriptors(batch);

            Assert.AreEqual(2, descriptors.Count);
            Assert.AreEqual(2, descriptors[0].Templates.Count);
            Assert.AreEqual(2, descriptors[1].Templates.Count);
            Assert.That(descriptors.Any(d => d.Templates.Contains("Stub\\_Widget.spark") && d.Templates.Contains("Shared\\ajax.spark")));
            Assert.That(descriptors.Any(d => d.Templates.Contains("Stub\\List.spark") && d.Templates.Contains("Shared\\default.spark")));
        }

        [Test]
        public void FileWithoutSparkExtensionAreIgnored()
        {
            var batch = new SparkBatchDescriptor();
            batch.For<StubController>();
            var descriptors = _factory.CreateDescriptors(batch);
            
            // no templates
            Assert.That(descriptors.SelectMany(d=>d.Templates).All(t=>!t.Contains("Helper")));

        }

    }
}