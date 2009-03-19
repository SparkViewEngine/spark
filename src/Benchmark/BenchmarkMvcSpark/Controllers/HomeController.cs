using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Benchmark;

namespace BenchmarkMvcSpark.Controllers
{
    public class HomeController : Controller
    {
        static BlogDao dao = new BlogDao();

        public ActionResult Index()
        {
            return View(dao.GetPost());
        }
    }
}
