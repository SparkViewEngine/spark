using System;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using AspNetMvc.CompileOnStart.Controllers;
using MvcContrib.SparkViewEngine;
using Spark;

namespace AspNetMvc.CompileOnStart
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


        public static void PrecompileViews(SparkViewFactory factory)
        {
            var viewsAssemblyName = typeof (Global).Assembly.GetName().Name + ".Views";

            try
            {
                // production case: load a precompiled assembly
                var assembly = Assembly.Load(viewsAssemblyName);
                factory.Engine.LoadBatchCompilation(assembly);
            }
            catch (FileNotFoundException)
            {
                // development case: precompile assemblies
                var batch = new SparkBatchDescriptor(viewsAssemblyName + ".dll");

                batch
                    .For<HomeController>()
                    .For<HomeController>().Layout("Ajax").Include("_Notification")
                    .For<AccountController>();

                var assembly = factory.Precompile(batch);

                // pre-production case: assembly copied to a known location (app_data) for deployment
                if (assembly != null && assembly.Location != null)
                {
                    var dataDirectory = Convert.ToString(AppDomain.CurrentDomain.GetData("DataDirectory"));

                    File.Copy(assembly.Location, Path.Combine(dataDirectory, viewsAssemblyName + ".dll"), true);
                }
            }
        }
    }
}
