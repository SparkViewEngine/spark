using System;
using System.Data;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Internationalization.Models;

namespace Internationalization.Controllers
{
    public class HomeController : Controller
    {
        public object Index()
        {
            var user = new UserInfo
                                   {
                                       Name = "Frankie",
                                       Culture = Application.GetSessionCulture(ControllerContext)
                                   };

            try
            {
                Thread.CurrentThread.CurrentCulture = 
                    CultureInfo.CreateSpecificCulture(user.Culture);
            }
            catch
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            }
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            ViewData["user"] = user;
            return View();
        }

        public object ChooseLanguage(string culture)
        {
            Application.SetSessionCulture(ControllerContext, culture);
            return RedirectToAction("index");
        }
    }
}
