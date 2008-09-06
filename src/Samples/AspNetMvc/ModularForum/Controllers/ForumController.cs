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
using ModularForum.Models;

namespace ModularForum.Controllers
{
    public class ForumController : Controller
    {
        private ForumRepository _repository;

        public ForumController()
        {
            _repository = new ForumRepository();
        }

        public object Index()
        {
            ViewData["forums"] = _repository.ListForums();
            return View();
        }

        public object Show(string id)
        {
            ViewData["forum"] = _repository.GetForum(id);
            return View();
        }
    }
}
