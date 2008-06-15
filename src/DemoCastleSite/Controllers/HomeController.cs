using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using Spark;

namespace DemoCastleSite.Controllers
{
    [Layout("Default")]
    public class HomeController : SmartDispatcherController
    {
        public void Index()
        {

        }

        public void Apply(string name, string address)
        {
            PropertyBag["caption"] = "Information submitted";
            PropertyBag["message"] = string.Format("Name {0}, Address {1}", name, address);
            PropertyBag["continue"] = new { controller = "Home", action = "Index" };
            RenderSharedView("common/simplemessage");
        }

        public void ShowPage(int page)
        {
            IList<string> data = new List<string>();
            for (int i = 100; i != 200; ++i)
                data.Add(i.ToString());

            
            


            PropertyBag["items"] = PaginationHelper.CreatePagination(data, 10, page);
        }

        public void Script()
        {

        }

        public IViewEngine ViewEngine { get; set; }

        public void ViewSource(string controllerName, string viewName, string masterName)
        {
            var entry = CompiledViewHolder.Current.Lookup(new CompiledViewHolder.Key { ControllerName = controllerName, MasterName = masterName, ViewName = viewName });

            PropertyBag["entry"] = entry;
        }
    }
}
