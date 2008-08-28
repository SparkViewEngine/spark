using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using MvcContrib.SparkViewEngine;
using MvcContrib.ViewFactories;
using NorthwindDemo.Controllers;
using NUnit.Framework;
using Spark;
using Spark.FileSystem;

namespace NorthwindDemo.Tests
{
    [TestFixture]
    public class ProductsControllerTester
    {
        [Test]
        public void TestCompiledViews()
        {
            // Use the Global class to initialize settings
            var builder = new ControllerBuilder();
            Global.RegisterControllerFactory(builder);
            var controllerFactory = (SparkControllerFactory)builder.GetControllerFactory();

            // Use the spark view factory to initialize an engine
            var viewFactory = new SparkViewFactory(controllerFactory.Settings)
                                  {
                                      ViewSourceLoader = new FileSystemViewSourceLoader(@"..\..\..\NorthwindDemo\Views")
                                  };

            var batch = new SparkBatchDescriptor();
            batch.For<ProductsController>();
            viewFactory.Precompile(batch);
        }

        [Test]
        public void TestCompiledViewsWithJustEngine()
        {
            // Make the same settings the web app provides.
            // (Assemblies are forced to be added because many aren't loaded 
            // in the nunit appdomain.)
            var settings = new SparkSettings()
                .SetDebug(false)
                .SetPageBaseType("MvcContrib.SparkViewEngine.SparkView")
                .AddNamespace("System")
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("System.Linq")
                .AddNamespace("System.Web.Mvc")
                .AddNamespace("NorthwindDemo.Models")
                .AddNamespace("NorthwindDemo.Views.Helpers")
                .AddAssembly("NorthwindDemo")
                .AddAssembly("System.Web.Mvc")
                .AddAssembly("MvcContrib")
                .AddAssembly("MvcContrib.SparkViewEngine")
                .AddAssembly("System.Web.Routing");

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
