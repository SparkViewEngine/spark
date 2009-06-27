using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Spark.Web.Mvc;

namespace IntegrationTestingViews
{
    public class Application
    {
        public void RegisterViewEngines(ViewEngineCollection engines)
        {
            if (engines == null) throw new ArgumentNullException("engines");

            SparkEngineStarter.RegisterViewEngine(engines);
        }

        public void RegisterRoutes(RouteCollection routes)
        {
            if (routes == null) throw new ArgumentNullException("routes");

            // default route
            routes.Add(new Route(
                           "{controller}/{action}/{id}",
                           new RouteValueDictionary(new { action = "Index", id = "" }),
                           new MvcRouteHandler()));
            
        }
    }
}