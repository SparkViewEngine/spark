using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;
using Castle.Windsor;

namespace Spark.Modules
{
    public interface IWebPackage
    {
        void Register(IWindsorContainer container, ICollection<RouteBase> routes, ICollection<IViewEngine> viewEngines);
    }
}
