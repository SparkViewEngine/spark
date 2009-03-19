using System;
using System.Collections.Generic;
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
using IronPythonViews.Models;
using System.Web.Mvc.Html;

namespace IronPythonViews.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var repos = new ProductRepository();
            ViewData["products"] = repos.GetProducts();
            ViewData["foo"] = "bar";
            ViewData["quux"] = 3 + 4 + 5;
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
            var showBirds = allBirds.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            var birdCount = allBirds.Count();
            
            ViewData["birds"] = new Page<Bird>
            {
                Items = showBirds,
                ItemCount = birdCount,

                CurrentPage = pageNumber,
                PageCount = (birdCount + pageSize - 1) / pageSize,

                FirstItemIndex = (pageNumber - 1) * pageSize
            };
            return View();
        }
    }

    public class Page<TItem>
    {
        public IEnumerable<TItem> Items { get; set; }
        public int ItemCount { get; set; }

        public int CurrentPage { get; set; }
        public int PageCount { get; set; }

        public int FirstItemIndex { get; set; }

        public bool HasPreviousPage { get { return CurrentPage > 1; } }
        public bool HasNextPage { get { return CurrentPage < PageCount; } }

    }
}
