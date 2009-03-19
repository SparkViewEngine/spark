using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace IronPythonViews
{
    public partial class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            RegisterViewEngine(ViewEngines.Engines);
            RegisterRoutes(RouteTable.Routes);
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