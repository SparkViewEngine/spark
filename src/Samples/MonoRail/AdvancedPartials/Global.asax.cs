using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;
using Castle.MonoRail.Framework.Routing;

namespace AdvancedPartials
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes(RoutingModuleEx.Engine);
        }

        private void RegisterRoutes(RoutingEngine engine)
        {
            engine.Add(new PatternRoute("/<controller>/[action]/[id]")
                .DefaultForAction().Is("index")
                .DefaultFor("id").Is(""));
            
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var path = Request.AppRelativeCurrentExecutionFilePath;
            if (string.Equals(path, "~/default.aspx", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(path, "~/"))
            {
                Context.RewritePath("~/home/index.castle");
            }
        }
    }
}