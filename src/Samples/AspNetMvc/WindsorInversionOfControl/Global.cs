using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using MvcContrib.ControllerFactories;
using MvcContrib.Services;
using MvcContrib.SparkViewEngine;
using MvcContrib.ViewFactories;
using Spark;
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
                .Configure(c=>c.LifeStyle.Is(LifestyleType.Transient)));

            // Place this container as the dependency resolver and hook it into
            // the controller factory mechanism
            DependencyResolver.InitializeWith(new WindsorDependencyResolver(container.Kernel));
            ControllerBuilder.Current.SetControllerFactory(typeof(IoCControllerFactory));


            // Some more components from the sample
            container.AddComponent<IViewSourceLoader, FileSystemViewSourceLoader>();
            container.AddComponent<ISampleRepository, SampleRepository>();
            container.AddComponent<INavRepository, NavRepository>();
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