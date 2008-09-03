using System.Linq;
using System.Web.Mvc;
using NorthwindDemo.Controllers;
using NUnit.Framework;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;

namespace NorthwindDemo.Tests
{
    [TestFixture]
    public class ProductsControllerTester
    {
        [Test]
        public void TestViewsWithBatchPrecompile()
        {
            // Use the Global class to initialize settings
            var engines = new ViewEngineCollection();
            Global.RegisterViewEngine(engines);

            var viewEngine = engines.OfType<SparkViewFactory>().First();

            var batch = new SparkBatchDescriptor();
            batch.For<ProductsController>();
            viewEngine.Precompile(batch);
        }

        [Test]
        public void TestCompiledViewsWithJustEngine()
        {
            // Make the same settings the web app provides.
            // (Assemblies are forced to be added because many aren't loaded 
            // in the nunit appdomain.)
            var settings = new SparkSettings()
                .SetDebug(false)
                .SetPageBaseType("Spark.Web.Mvc.SparkView")
                .AddNamespace("System")
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("System.Linq")
                .AddNamespace("System.Web.Mvc")
                .AddNamespace("NorthwindDemo.Models")
                .AddNamespace("NorthwindDemo.Views.Helpers")
                .AddAssembly("NorthwindDemo")
                .AddAssembly("System.Web.Mvc")
                .AddAssembly("Spark.Web.Mvc");

            // create an engine
            var engine = new SparkViewEngine(settings)
                             {
                                 ViewFolder = new FileSystemViewFolder(@"..\..\..\NorthwindDemo\Views")
                             };

            // Generate and compile a bunch of views
            engine.CreateEntry(new SparkViewDescriptor()
                                   .SetTargetNamespace("NorthwindDemo.Controllers")
                                   .AddTemplate("Products/Edit.spark")
                                   .AddTemplate("Shared/Application.spark"));

            engine.CreateEntry(new SparkViewDescriptor()
                                   .SetTargetNamespace("NorthwindDemo.Controllers")
                                   .AddTemplate("Products/Categories.spark")
                                   .AddTemplate("Shared/Application.spark"));

            engine.CreateEntry(new SparkViewDescriptor()
                                   .SetTargetNamespace("NorthwindDemo.Controllers")
                                   .AddTemplate("Products/Detail.spark")
                                   .AddTemplate("Shared/Application.spark"));

            engine.CreateEntry(new SparkViewDescriptor()
                                   .SetTargetNamespace("NorthwindDemo.Controllers")
                                   .AddTemplate("Products/ListingByCategory.spark")
                                   .AddTemplate("Shared/Application.spark"));

        }
    }
}
