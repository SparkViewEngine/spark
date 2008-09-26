using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Container;
using Castle.MonoRail.Views.Spark;
using PrecompiledViews.Controllers;
using Spark;

namespace PrecompiledViews
{
    public class Global : System.Web.HttpApplication, IMonoRailContainerEvents
    {
        protected void Application_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var path = Request.AppRelativeCurrentExecutionFilePath;
            if (string.Equals(path, "~/default.aspx", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(path, "~/"))
            {
                Context.RewritePath("~/home/index.ashx");
            }
        }

        void IMonoRailContainerEvents.Created(IMonoRailContainer container)
        {
        }

        void IMonoRailContainerEvents.Initialized(IMonoRailContainer container)
        {
            var precompiled = Assembly.Load("PrecompiledViews.Views");

            var factory = new SparkViewFactory();
            factory.Service(container);
            factory.Engine.LoadBatchCompilation(precompiled);
        }

    }
}