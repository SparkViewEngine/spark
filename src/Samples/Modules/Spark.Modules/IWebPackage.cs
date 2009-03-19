using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;

namespace Spark.Modules
{
    public interface IWebPackage
    {
        void Register(IKernel container, ICollection<RouteBase> routes, ICollection<IViewEngine> viewEngines);
    }
}
