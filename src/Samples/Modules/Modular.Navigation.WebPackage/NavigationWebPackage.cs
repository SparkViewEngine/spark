using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;
using Spark.Modules;

namespace Modular.Navigation.WebPackage
{
    public class NavigationWebPackage : WebPackageBase
    {
        public override void Register(IKernel container, ICollection<RouteBase> routes, ICollection<IViewEngine> viewEngines)
        {
            RegisterStandardArea(container, routes, viewEngines, "Navigation");
        }
    }
}
