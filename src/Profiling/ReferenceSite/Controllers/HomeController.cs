using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReferenceSite.Model;

namespace ReferenceSite.Controllers
{
    public class HomeController : Controller
    {
        static readonly DataContext _dataContext;
        static HomeController()
        {
            _dataContext = new DataContext();
            _dataContext.Initialize();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AllStories()
        {
            return View(_dataContext.Story);
        }

        public ActionResult ShowRegions()
        {
            return View();
        }
    }
}
