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
    /// <summary>
    /// Shows the use of index.spark + defaultlayout.spark, 
    /// and alternate.aspx + defaultlayout.master, both of which
    /// render a mix of spark, aspx, and ascx partial files.
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View("Index", "DefaultLayout");
        }

        public ActionResult Alternate()
        {
            return View("Alternate", "DefaultLayout");
        }
    }
}
