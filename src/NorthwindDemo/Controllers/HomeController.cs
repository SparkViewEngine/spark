using System;
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
using System.Web.Mvc;
using Spark;

namespace NorthwindDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewGeneratedSource(string masterName, string controllerName, string viewName)
        {
            var key = new CompiledViewHolder.Key { MasterName = masterName, ControllerName = controllerName, ViewName = viewName };
            var entry = CompiledViewHolder.Current.Lookup(key);
            return View(entry);
        }
    }
}
