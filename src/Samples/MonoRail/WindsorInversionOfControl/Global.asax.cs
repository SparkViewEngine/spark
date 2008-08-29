using System;
using System.Reflection;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Container;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Views.Spark;
using Castle.Windsor;
using Spark;

namespace WindsorInversionOfControl
{
    public partial class Global : System.Web.HttpApplication, IContainerAccessor
    {
        static private IWindsorContainer _container;
        public IWindsorContainer Container
        {
            get { return _container; }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            _container = new WindsorContainer();
            RegisterFacilities(_container);
            RegisterComponents(_container);
            RegisterRoutes(RoutingModuleEx.Engine);
        }

    }
}