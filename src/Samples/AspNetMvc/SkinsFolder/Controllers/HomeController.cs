using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SkinsFolder.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SetTheme(string name)
        {
            Response.SetCookie(new HttpCookie("theme", name));
            return RedirectToAction("ThemeSelected");
        }

        public ActionResult ThemeSelected()
        {
            return View();
        }
    }
}
