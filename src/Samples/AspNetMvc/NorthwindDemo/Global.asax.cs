using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.SparkViewEngine;

namespace NorthwindDemo
{
    public class GlobalApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            // Note: Change the URL to "{controller}.mvc/{action}/{id}" to enable
            //       automatic support on IIS6 and IIS7 classic mode

            routes.MapRoute("mvcroute", "{controller}/{action}/{id}"
                , new { controller="products", action = "Index", id = "" }
                , new { controller=@"[^\.]*"});
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes(RouteTable.Routes);
            ViewEngines.Engines.Add(new SparkViewFactory());
        }
    }
}