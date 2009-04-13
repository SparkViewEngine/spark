using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace SkinsFolder
{
    public partial class Global : System.Web.HttpApplication
    {
        private readonly Application _application = new Application();

        protected void Application_Start(object sender, EventArgs e)
        {
           _application.RegisterViewEngine(ViewEngines.Engines);
           _application.RegisterRoutes(RouteTable.Routes);
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