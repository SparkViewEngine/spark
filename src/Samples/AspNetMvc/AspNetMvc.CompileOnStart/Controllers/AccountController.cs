using System.Web.Mvc;

namespace AspNetMvc.CompileOnStart.Controllers
{
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
