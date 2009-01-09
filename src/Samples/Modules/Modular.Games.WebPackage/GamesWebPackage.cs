using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Spark.FileSystem;
using Spark.Modules;
using Spark.Web.Mvc;

namespace Modular.Games.WebPackage
{
    public class GamesWebPackage : IWebPackage
    {
        public void Register(IWindsorContainer container, ICollection<RouteBase> routes, ICollection<IViewEngine> viewEngines)
        {
            container
                .Register(AllTypes
                              .Of<IController>()
                              .FromAssembly(typeof(GamesWebPackage).Assembly)
                              .Configure(component => component
                                                          .Named("games." +
                                                                 component.ServiceType.Name.ToLowerInvariant())
                                                          .LifeStyle.Transient));

            routes.Add(new Route("{area}/{controller}/{action}",
                                 new RouteValueDictionary(new { action = "index" }),
                                 new RouteValueDictionary(new { area = "games" }),
                                 new MvcRouteHandler()));


            var viewFolder = new EmbeddedViewFolder(typeof(GamesWebPackage).Assembly, "Modular.Games.WebPackage.Views");
            var sparkViewFactory = viewEngines.OfType<SparkViewFactory>().First();

            sparkViewFactory.ViewFolder = sparkViewFactory.ViewFolder
                .Append(new SubViewFolder(viewFolder, "games"))
                .Append(new SubViewFolder(viewFolder, "Shared\\games"));
        }
    }
}
