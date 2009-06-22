using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using IntegrationTestingViews;

namespace IntegrationTestingViews
{
    public class Global : HttpApplication
    {
        private static readonly Application _application = new Application();

        protected void Application_Start(object sender, EventArgs e)
        {
            _application.RegisterViewEngines(ViewEngines.Engines);
            _application.RegisterRoutes(RouteTable.Routes);
        }


        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // this ensures Default.aspx will be processed
            HttpContext context = ((HttpApplication) sender).Context;
            string relativeFilePath = context.Request.AppRelativeCurrentExecutionFilePath;
            if (relativeFilePath == "~/" ||
                string.Equals(relativeFilePath, "~/default.aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                context.RewritePath("~/Home");
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}