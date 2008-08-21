using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using AspNetMvcIoC.Models;
using Castle.Windsor;
using MvcContrib.Castle;
using MvcContrib.ControllerFactories;
using MvcContrib.Services;
using MvcContrib.SparkViewEngine;
using MvcContrib.ViewFactories;
using Spark;

namespace AspNetMvcIoC
{
    public partial class Global
    {
        public void ConfigureIoC()
        {
            // create a Windsor container with various component parameters established
            var container = new WindsorContainer(Server.MapPath("~/castle.config"));

            // Replaces the default IViewEngine. 
            container.AddComponent<IViewEngine, SparkViewFactory>();
            container.AddComponent<ISparkViewEngine, SparkViewEngine>();
            container.AddComponent<IViewActivatorFactory, WindsorViewActivator>();

            // Add anything descended from IController/Controller 
            container.RegisterControllers(typeof(Global).Assembly);

            // Place this container as the dependency resolver and hook it into
            // the controller factory mechanism
            DependencyResolver.InitializeWith(new WindsorDependencyResolver(container));
            ControllerBuilder.Current.SetControllerFactory(typeof(IoCControllerFactory));


            // The following demonstrates a few more techniques, but aren't part of the
            // bare-minimum code for IoC

            // Throw in a view source loader and a data access component. 
            // These dependencies are resolved as needed.
            container.AddComponent<IViewSourceLoader, FileSystemViewSourceLoader>();
            container.AddComponent<ISampleRepository, SampleRepository>();
            container.AddComponent<INavRepository, NavRepository>();


            // Example of providing settings as ISparkSettings instead 
            // of using the <spark> in .config... If you use the <spark> section 
            // don't register this component instance.

            var settings = new SparkSettings()
                .SetDebug(true)
                .SetPageBaseType("AspNetMvcIoC.Views.View")
                .AddNamespace("System.Collections.Generic")
                .AddNamespace("AspNetMvcIoC.Models");
            container.Kernel.AddComponentInstance<SparkSettings>(typeof(ISparkSettings), settings);

        }

        public static void AddRoutes(RouteCollection routes)
        {
            routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(new { action = "Index", id = "" }),
            });

            routes.Add(new Route("Default.aspx", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(new { controller = "Home", action = "Index", id = "" }),
            });

        }
    }
}
