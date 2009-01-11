using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
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
