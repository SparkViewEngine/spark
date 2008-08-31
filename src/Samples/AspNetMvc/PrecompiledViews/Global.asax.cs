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

namespace PrecompiledViews
{
    public partial class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var controllerFactory = RegisterControllerFactory(ControllerBuilder.Current);

            RegisterRoutes(RouteTable.Routes);

            var viewFactory = new SparkViewFactory(controllerFactory.Settings)
                                  {
                                      ViewSourceLoader = controllerFactory.ViewSourceLoader
                                  };

            LoadPrecompiledViews(viewFactory);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var path = Request.AppRelativeCurrentExecutionFilePath;
            if (string.Equals(path, "~/default.aspx", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(path, "~/"))
            {
                Context.RewritePath("~/home");
            }
        }

    }
}