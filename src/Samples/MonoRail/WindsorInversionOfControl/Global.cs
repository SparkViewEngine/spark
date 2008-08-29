using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.WindsorExtension;
using Castle.Windsor;
using Spark;
using WindsorInversionOfControl.Models;

namespace WindsorInversionOfControl
{
    public partial class Global 
    {
        public static void RegisterFacilities(IWindsorContainer container)
        {
            container.AddFacility("mr", new MonoRailFacility());
            container.AddFacility("logging", new LoggingFacility(LoggerImplementation.Trace));
        }

        public static void RegisterComponents(IWindsorContainer container)
        {
            container.Register(
                // add all the controllers
                AllTypes.Of<Controller>().FromAssemblyNamed("WindsorInversionOfControl"),

                // this component will result in the views also being registered in the container
                Component.For<IViewActivatorFactory>().ImplementedBy<ViewActivator>(),

                // here's an example of a component used by the views. see the View class.
                Component.For<INavProvider>().ImplementedBy<NavProvider>());
        }

        public static void RegisterRoutes(IRoutingRuleContainer engine)
        {
            engine.Add(new PatternRoute("/<controller>/[action]/[id]")
                .DefaultForAction().Is("index")
                .DefaultFor("id").Is(""));
        }
    }
}
