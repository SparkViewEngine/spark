using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;
using MvcContrib.SparkViewEngine;

namespace AspNetMvc.CompileOnStart
{
    public partial class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var controllerFactory = RegisterControllerFactory(ControllerBuilder.Current);

            RegisterRoutes(RouteTable.Routes);

            var viewFactory = new SparkViewFactory(controllerFactory.Settings) { ViewSourceLoader = controllerFactory.ViewSourceLoader };

            PrecompileViews(viewFactory);
        }
    }
}
