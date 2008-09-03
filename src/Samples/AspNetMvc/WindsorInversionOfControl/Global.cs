using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;
using WindsorInversionOfControl.Models;

namespace WindsorInversionOfControl
{
    public partial class Global
    {
        public void ConfigureIoC()
        {
            // create a Windsor container with various component parameters established
            var container = new WindsorContainer(Server.MapPath("~/castle.config"));

            // Replaces the default IViewEngine. 
            container.AddComponent<IViewEngine, SparkViewFactory>();
            container.AddComponent<IViewActivatorFactory, WindsorViewActivator>();

            // Add anything descended from IController/Controller 
            container.Register(
                AllTypes.Of<IController>()
                .FromAssembly(typeof(Global).Assembly)
                .Configure(c=>c.LifeStyle.Transient.Named(c.Implementation.Name.ToLowerInvariant())));

            // Some more components from the sample
            container.AddComponent<IViewFolder, FileSystemViewFolder>();
            container.AddComponent<ISampleRepository, SampleRepository>();
            container.AddComponent<INavRepository, NavRepository>();

            // Place this container as the dependency resolver and hook it into
            // the controller factory mechanism
            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container.Kernel));
            ViewEngines.Engines.Add(container.Resolve<IViewEngine>());
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