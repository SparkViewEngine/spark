using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using MacrosSample.Models;

namespace MacrosSample.Controllers {
    [HandleError]
    public class HomeController : Controller {        

        public ActionResult Index() {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About() {
            return View();
        }

        public ActionResult Config() {
            // this is often a bad idea
            return View(XElement.Load(Server.MapPath("~/web.config")));
        }
    }
}
