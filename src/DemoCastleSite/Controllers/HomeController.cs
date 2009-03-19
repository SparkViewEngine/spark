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
using Spark.FileSystem;

namespace SparkCastleDemo.Controllers
{
    public class MagicHelper : AbstractHelper
    {
        public string Tada(string foo, int bar)
        {
            return foo + (bar * 5);
        }
    }

    [Layout("Default")]
    [Helper(typeof(MagicHelper), "Magic")]
    public class HomeController : SmartDispatcherController
    {
        public void Index()
        {
        }

        public void Clientside()
        {
            var engine = new SparkViewEngine
                             {
                                 ViewFolder = new VirtualPathProviderViewFolder("~/Views")
                             };

            var entry = engine.CreateEntry(new SparkViewDescriptor()
                .SetLanguage(LanguageType.Javascript)
                .AddTemplate("home\\_widget.spark"));

            Response.ContentType = "text/javascript";
            RenderText(entry.SourceCode);
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

        //public void ViewSource(string controllerName, string viewName, string masterName)
        //{
        //    var descriptor = new SparkViewDescriptor
        //    {
        //        ControllerName = controllerName,
        //        MasterName = masterName,
        //        ViewName = viewName
        //    };

        //    var entry = CompiledViewHolder.Current.Lookup(new CompiledViewHolder.Key { Descriptor = descriptor });

        //    PropertyBag["entry"] = entry;
        //}

        [Layout("Sidebars", "Default")]
        public void Nested()
        {

        }
    }
}