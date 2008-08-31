using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;

namespace WindsorInversionOfControl
{
    public partial class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            ConfigureIoC();
            AddRoutes(RouteTable.Routes);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var path = Request.AppRelativeCurrentExecutionFilePath;
            if (string.Equals(path, "~/default.aspx", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(path, "~/"))
            {
                Context.RewritePath("~/home/index");
            }
        }
    }
}