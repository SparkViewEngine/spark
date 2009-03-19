using System.Web.Mvc;
using Castle.Core.Logging;

namespace Modular.Fleet.WebPackage.Controllers
{
    public class HomeController : Controller
    {
        private ILogger _logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Play()
        {
            Logger.Info("Game's Play invoked");

            return View("Index");
        }
    }
}
