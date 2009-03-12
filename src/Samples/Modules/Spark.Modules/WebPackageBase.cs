using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Spark.FileSystem;
using Spark.Web.Mvc;

namespace Spark.Modules
{
    public abstract class WebPackageBase : IWebPackage
    {
        public abstract void Register(IKernel container, ICollection<RouteBase> routes, ICollection<IViewEngine> viewEngines);

        protected void RegisterStandardArea(IKernel container, ICollection<RouteBase> routes, ICollection<IViewEngine> viewEngines, string areaName)
        {
            var assembly = GetType().Assembly;
            RegisterStandardComponents(container, assembly, areaName);
            RegisterStandardRoutes(routes, assembly, areaName);
            RegisterStandardViewFolders(viewEngines, assembly, areaName);
        }

        public void RegisterStandardComponents(IKernel container, Assembly assembly, string areaName)
        {
            container
                .Register(AllTypes
                              .FromAssembly(assembly)
                              .BasedOn<IController>()
                              .Configure(component => component
                                                          .Named(areaName.ToLowerInvariant() + "." +
                                                                 component.ServiceType.Name.ToLowerInvariant())
                                                          .LifeStyle.Transient))

                .Register(AllTypes
                              .FromAssembly(assembly)
                              .BasedOn<IBlock>()
                              .Configure(component => component
                                                          .Named(component.ServiceType.Name.ToLowerInvariant())
                                                          .LifeStyle.Transient))

                .Register(AllTypes
                              .FromAssembly(assembly).IncludeNonPublicTypes()
                              .BasedOn<IService>()
                              .WithService.FromInterface(typeof(IService)));
        }

        public void RegisterStandardRoutes(ICollection<RouteBase> routes, Assembly assembly, string areaName)
        {
            routes.Add(new Route("{area}/{controller}/{action}",
                                 new RouteValueDictionary(new { controller = "home", action = "index" }),
                                 new RouteValueDictionary(new { area = areaName }),
                                 new MvcRouteHandler()));

            routes.Add(new Route("content/{area}/{*resource}",
                                 new RouteValueDictionary(),
                                 new RouteValueDictionary(new { area = areaName }),
                                 new EmbeddedContentRouteHandler(assembly, assembly.GetName().Name + ".Content")));
        }

        public void RegisterStandardViewFolders(ICollection<IViewEngine> viewEngines, Assembly assembly, string areaName)
        {
            var viewFolder = new EmbeddedViewFolder(assembly, assembly.GetName().Name + ".Views");
            var sparkViewFactory = viewEngines.OfType<SparkViewFactory>().First();

            sparkViewFactory.ViewFolder = sparkViewFactory.ViewFolder
                .Append(new SubViewFolder(viewFolder, areaName))
                .Append(new SubViewFolder(viewFolder, "Shared\\" + areaName));

        }

    }
}
