using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;

namespace Spark.Modules
{
    public interface IWebPackageManager
    {
        void LocatePackages();
        void RegisterPackages(ICollection<RouteBase> routes, ICollection<IViewEngine> engines);
    }

    public class WebPackageManager : IWebPackageManager
    {
        private readonly IKernel _kernel;

        public WebPackageManager(IKernel kernel)
        {
            _kernel = kernel;
        }

        public void LocatePackages()
        {
            // NOTE - this could be a place to rely on a package locating service 
            // rather than code in the discovery strategy

            var searchPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath);
            foreach(var assemblyName in Directory.GetFiles(searchPath, "*.dll").Select(path=>Path.GetFileNameWithoutExtension(path)))
            {
                _kernel
                    .Register(AllTypes
                                  .Of<IWebPackage>()
                                  .FromAssemblyNamed(assemblyName)
                                  .WithService.FromInterface(typeof(IWebPackage)));
            }
        }

        public void RegisterPackages(ICollection<RouteBase> routes, ICollection<IViewEngine> engines)
        {
            IEnumerable<IHandler> remainingPackages = _kernel.GetHandlers(typeof(IWebPackage));

            while (remainingPackages.Count() != 0)
            {
                var validPackages = remainingPackages.Where(handler => handler.CurrentState == HandlerState.Valid).ToArray();
                if (validPackages.Count() == 0)
                    break;

                foreach (var handler in validPackages)
                {
                    var package = _kernel.Resolve<IWebPackage>(handler.ComponentModel.Name);
                    package.Register(_kernel, routes, engines);
                    _kernel.ReleaseComponent(package);
                }

                remainingPackages = remainingPackages.Except(validPackages).ToArray();
            }

            //TODO: throw a detail-rich exception
            if (remainingPackages.Count() != 0)
                throw new ApplicationException("Web packages have unresolved dependencies");
        }

    }
}
