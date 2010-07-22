using System;
using System.Web.Mvc;
using JavascriptViewResultSample.Models;
using JavascriptViewResultSample.Components.Paging;
using JavascriptViewResultSample.Services.Data;
using Spark.Web.Mvc;

namespace JavascriptViewResultSample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List(int? page)
        {
            var pager = new Pager(page, 24, 6);
            PagedList<RegisteredUser> RegisteredUserList = InMemoryDataService.GetAll(pager);
            ViewData["list"] = RegisteredUserList;
            return View(RegisteredUserList);
        }

        public ActionResult ListPage(int page)
        {
            var pager = new Pager(page, 24, 6);
            PagedList<RegisteredUser> RegisteredUserList = InMemoryDataService.GetAll(pager);
            return Json(RegisteredUserList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListPageView()
        {
            return new JavascriptViewResult { ViewName = "_ListPage" };
        }
    }
}
