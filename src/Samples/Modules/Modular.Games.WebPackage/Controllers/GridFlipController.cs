using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Modular.Games.WebPackage.Controllers
{
    public class GridFlipController : Controller
    {
        public ActionResult Index()
        {
            return View("ShowGrid");
        }

        public ActionResult Reset()
        {
            return View("ShowGrid");
        }
    }
}
