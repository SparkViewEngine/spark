using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Facilities.Logging;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Spark.Modules;
using Spark.Web.Mvc;

namespace Modular.WebHost
{
    public class Application
    {
        public void RegisterFacilities(IWindsorContainer container)
        {
            container.AddFacility("logging", new LoggingFacility(LoggerImplementation.Trace));
        }

        public void RegisterComponents(IWindsorContainer container)
        {
            container
                .Register(Component
                              .For<IControllerFactory>()
                              .ImplementedBy<ModularControllerFactory>()
                              .LifeStyle.Singleton)

                .Register(Component
                              .For<IWebPackageManager>()
                              .ImplementedBy<WebPackageManager>()
                              .LifeStyle.Transient)

                .Register(AllTypes
                              .Of<IController>()
                              .FromAssembly(typeof(Application).Assembly)
                              .Configure(component => component
                                                          .Named(component.ServiceType.Name.ToLowerInvariant())
                                                          .LifeStyle.Transient));
        }

        public void RegisterViewEngine(ICollection<IViewEngine> engines)
        {
            SparkEngineStarter.RegisterViewEngine(engines);
        }

        public void RegisterPackages(IWindsorContainer container, ICollection<RouteBase> routes, ICollection<IViewEngine> engines)
        {
            var manager = container.Resolve<IWebPackageManager>();
            manager.LocatePackages();
            manager.RegisterPackages(routes, engines);
            container.Release(manager);
        }

        public void RegisterRoutes(ICollection<RouteBase> routes)
        {
            routes.Add(new Route("{controller}/{action}",
                                 new RouteValueDictionary(new { controller = "home", action = "index" }),
                                 new MvcRouteHandler()));
        }
    }
}
