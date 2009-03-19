using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Castle.Core.Logging;

namespace Modular.WebHost.Controllers
{
    public class HomeController : Controller
    {
        private ILogger _logger = NullLogger.Instance;
        public ILogger Logger { get { return _logger; } set { _logger = value; } }

        public ActionResult Index()
        {
            Logger.Debug("Called Index in main site's Home controller");

            return View();
        }
    }
}
