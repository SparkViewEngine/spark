using System;
using System.Web.Routing;
using System.Web.Mvc;

namespace NorthwindDemo
{
    public partial class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.SetData("SQLServerCompactEditionUnderWebHosting", true);

            RegisterControllerFactory(ControllerBuilder.Current);
            
            RegisterRoutes(RouteTable.Routes);

            PrecompileViews(ControllerBuilder.Current);
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