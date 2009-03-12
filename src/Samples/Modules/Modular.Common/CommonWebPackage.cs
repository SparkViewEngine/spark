using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;
using Spark.Modules;

namespace Modular.Common
{
    public class CommonWebPackage : WebPackageBase
    {
        public override void Register(IKernel container, ICollection<RouteBase> routes, ICollection<IViewEngine> viewEngines)
        {
            var assembly = typeof(CommonWebPackage).Assembly;

            RegisterStandardComponents(container, assembly, null);
        }
    }
}
