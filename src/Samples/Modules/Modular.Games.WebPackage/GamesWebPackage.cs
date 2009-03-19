using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;
using Modular.Common.Services;
using Modular.Navigation.WebPackage.Models;
using Modular.Navigation.WebPackage.Services;
using Spark.Modules;

namespace Modular.Games.WebPackage
{
    public class GamesWebPackage : WebPackageBase
    {
        private readonly IGameRegistry _gameRegistry;
        private readonly INavRegistry _navRegistry;
        private readonly ISideRegistry _sideRegistry;

        public GamesWebPackage(IGameRegistry gameRegistry, INavRegistry navRegistry, ISideRegistry sideRegistry)
        {
            _gameRegistry = gameRegistry;
            _navRegistry = navRegistry;
            _sideRegistry = sideRegistry;
        }

        public override void Register(IKernel container, ICollection<RouteBase> routes, ICollection<IViewEngine> viewEngines)
        {
            var areaName = "Games";

            RegisterStandardArea(container, routes, viewEngines, areaName);

            _gameRegistry.AddGame("Grid Flip", new { area = areaName, controller = "GridFlip" });
            _gameRegistry.AddGame("Letter Guessing", new { area = areaName, controller = "Hangman" });
            _navRegistry.AddItem(new NavItem
            {
                Weight = 5,
                Caption = "Games",
                Action = "Index",
                LinkValues = new { area = areaName, controller = "Home" }
            });
            _navRegistry.AddItem(new NavItem
            {
                Weight = 6,
                Caption = "High Scores",
                Action = "Scores",
                LinkValues = new { area = areaName, controller = "Home" }
            });
            _sideRegistry.AddItem(new SideItem
                                      {
                                          BlockName="HighScore",
                                          Weight=2
                                      });
        }
    }
}
