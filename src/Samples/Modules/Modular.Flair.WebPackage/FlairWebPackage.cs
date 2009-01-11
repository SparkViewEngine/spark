using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;
using Castle.Windsor;
using Modular.Navigation.WebPackage.Models;
using Modular.Navigation.WebPackage.Services;
using Spark.Modules;

namespace Modular.Flair.WebPackage
{
    public class FlairWebPackage : WebPackageBase
    {
        private readonly ISideRegistry _sideRegistry;

        public FlairWebPackage(ISideRegistry sideRegistry)
        {
            _sideRegistry = sideRegistry;
            DidYouKnowWeight = 5;
        }

        public int DidYouKnowWeight { get; set; }

        public override void Register(IKernel container, ICollection<RouteBase> routes, ICollection<IViewEngine> viewEngines)
        {
            _sideRegistry.AddItem(new SideItem {BlockName = "DidYouKnow", Weight = DidYouKnowWeight});

            RegisterStandardArea(container, routes, viewEngines, "Flair");
        }

    }
}
