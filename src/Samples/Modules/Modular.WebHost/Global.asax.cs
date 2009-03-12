using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Castle.Windsor;

namespace Modular.WebHost
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var container = new WindsorContainer(Server.MapPath("~/config/windsor.config"));

            var app = new Application();
            app.RegisterFacilities(container);
            app.RegisterComponents(container);
            app.RegisterViewEngine(ViewEngines.Engines);
            app.RegisterPackages(container, RouteTable.Routes, ViewEngines.Engines);
            app.RegisterRoutes(RouteTable.Routes);

            ControllerBuilder.Current.SetControllerFactory(container.GetService<IControllerFactory>());
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var path = Request.AppRelativeCurrentExecutionFilePath;
            if (string.Equals(path, "~/default.aspx", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(path, "~/"))
            {
                Context.RewritePath("~/Home");
            }
        }
    }
}