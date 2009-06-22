using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.MobileControls;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;
using System.Collections.Generic;

namespace IntegrationTestingViews.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewData["Message"] = "foo";
            return View(); 
        }

        public ActionResult WithPartial()
        {
            return View();
        }
    }
}