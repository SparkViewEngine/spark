using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace PartialFiles.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //HtmlHelper html;
            //html.RenderPartial();
            
            return View("Index", "Default");
        }
    }
}
