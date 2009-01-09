using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace Spark.Modules
{
    public class WebPackageManager
    {
        public void LocateAssemblyPackages(IWindsorContainer container)
        {
            var searchPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath);
            foreach(var assemblyName in Directory.GetFiles(searchPath, "*.dll").Select(path=>Path.GetFileNameWithoutExtension(path)))
            {
                container
                    .Register(AllTypes
                                  .Of<IWebPackage>()
                                  .FromAssemblyNamed(assemblyName)
                                  .WithService.FromInterface(typeof(IWebPackage)));
            }
        }

        public void RegisterPackages(IWindsorContainer container, ICollection<RouteBase> routes, ICollection<IViewEngine> engines)
        {
            IEnumerable<IHandler> remainingPackages = container.Kernel.GetHandlers(typeof(IWebPackage));

            while (remainingPackages.Count() != 0)
            {
                var validPackages = remainingPackages.Where(handler => handler.CurrentState == HandlerState.Valid);
                if (validPackages.Count() == 0)
                    break;

                foreach (var handler in validPackages)
                {
                    var package = container.Resolve<IWebPackage>(handler.ComponentModel.Name);
                    package.Register(container, routes, engines);
                    container.Release(package);
                }

                remainingPackages = remainingPackages.Except(validPackages);
            }

            //TODO: throw a detail-rich exception
            if (remainingPackages.Count() != 0)
                throw new ApplicationException("Web packages have unresolved dependencies");
        }

    }
}
