using System.Web.Mvc;
using WindsorInversionOfControl.Models;

namespace WindsorInversionOfControl.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISampleRepository _repository;

        public HomeController(ISampleRepository repository)
        {
            _repository = repository;
            
            //default intro message
            IntroMessage = "Hello World";
        }

        public string IntroMessage { get; set; }

        public ActionResult Index()
        {
            ViewData["Intro"] = IntroMessage;
            ViewData["Products"] = _repository.GetProducts();

            return View("Index");
        }
    }
}