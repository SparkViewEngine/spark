using System.Web.Mvc;
using Spark;

namespace PrecompiledViews.Controllers
{
    [Precompile]
    [Precompile("_Notification", "Ajax")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult List()
        {
            return View("List");
        }

        public ActionResult Detail()
        {
            return View("Detail");
        }

        public ActionResult Search()
        {
            return View("Search");
        }

        public ActionResult Notification()
        {
            return View("_Notification", "Ajax");
        }
    }
}