using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace ActionSample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var names = Directory.GetFiles(Server.MapPath("~/App_Data")).Select(path => Path.GetFileNameWithoutExtension(path));
            return View(names);
        }

        public ActionResult Invoice(string id)
        {
            var doc = XDocument.Load(Server.MapPath("~/App_Data/" + id + ".xml"));
            return View(doc.Root);
        }
    }
}
