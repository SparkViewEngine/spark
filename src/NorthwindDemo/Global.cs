using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.SparkViewEngine;

namespace NorthwindDemo
{
    public partial class Global
    {
        public static void RegisterControllerFactory(ControllerBuilder builder)
        {
            builder.SetControllerFactory(new SparkControllerFactory());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            // Note: Change the URL to "{controller}.mvc/{action}/{id}" to enable
            //       automatic support on IIS6 and IIS7 classic mode

            routes.MapRoute("mvcroute", "{controller}/{action}/{id}"
                , new { controller = "products", action = "Index", id = "" }
                , new { controller = @"[^\.]*" });
        }
    }
}
