using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;
using Modular.Common.Services;
using Modular.Navigation.WebPackage.Models;
using Modular.Navigation.WebPackage.Services;
using Spark.Modules;

namespace Modular.Fleet.WebPackage
{
    public class FleetWebPackage : WebPackageBase
    {
        private readonly IGameRegistry _gameRegistry;
        private readonly ISideRegistry _sideRegistry;

        public FleetWebPackage(IGameRegistry gameRegistry, ISideRegistry sideRegistry)
        {
            _gameRegistry = gameRegistry;
            _sideRegistry = sideRegistry;
        }

        public override void Register(
            IKernel container,
            ICollection<RouteBase> routes,
            ICollection<IViewEngine> viewEngines)
        {
            RegisterStandardArea(container, routes, viewEngines, "Fleet");

            _gameRegistry.AddGame("Star Fleet", new { area = "Fleet", controller = "Home" });

            _sideRegistry.AddItem(new SideItem { BlockName = "FleetTeaser", Weight = 8 });
        }
    }
}
