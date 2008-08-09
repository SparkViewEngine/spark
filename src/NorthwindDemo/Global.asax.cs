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
        }

    }
}