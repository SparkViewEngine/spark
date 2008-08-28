using System;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.SparkViewEngine;
using MvcContrib.SparkViewEngine.Install;
using PrecompiledViews.Controllers;
using Spark;

namespace PrecompiledViews
{
    public partial class Global
    {

        public static SparkControllerFactory RegisterControllerFactory(ControllerBuilder builder)
        {
            var factory = new SparkControllerFactory();
            builder.SetControllerFactory(factory);
            return factory;
        }


        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
                           {
                               Defaults = new RouteValueDictionary(new { action = "Index", id = "" }),
                           });

            routes.Add(new Route("Default.aspx", new MvcRouteHandler())
                           {
                               Defaults = new RouteValueDictionary(new { controller = "Home", action = "Index", id = "" }),
                           });
        }

        public static void LoadPrecompiledViews(SparkViewFactory factory)
        {
                factory.Engine.LoadBatchCompilation(Assembly.Load("Precompiled"));
        }
    }
}