using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using AspNetMvcIoC.Models;

namespace AspNetMvcIoC.Controllers
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
