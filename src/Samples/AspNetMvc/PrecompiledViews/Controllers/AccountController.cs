using System.Web.Mvc;
using Spark;

namespace PrecompiledViews.Controllers
{
    [Precompile]
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View("Login");
        }

        public ActionResult Logout()
        {
            return View("Logout");
        }

        public ActionResult Register()
        {
            return View("Register");
        }
    }
}