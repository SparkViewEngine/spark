using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdvancedPartials.Models;

namespace AdvancedPartials.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult AddingAtPlaceholders()
        {
            return View();
        }

        public ActionResult StyleGuide()
        {
            return View();
        }

        public ActionResult Boxes()
        {
            return View();
        }

        public ActionResult PagingAndRepeater(int? id)
        {
            var pageNumber = id ?? 1;
            var pageSize = 10;

            var repos = new BirdRepository();
            var allBirds = repos.GetBirds();
            var showBirds = allBirds.Skip((pageNumber - 1)*pageSize).Take(pageSize);
            var birdCount = allBirds.Count();

            return View(new Page<Bird>
                        {
                            Items = showBirds,
                            ItemCount = birdCount,

                            CurrentPage = pageNumber,
                            PageCount = (birdCount + pageSize - 1)/pageSize,

                            FirstItemIndex = (pageNumber - 1)*pageSize
                        });
        }
    }
}
