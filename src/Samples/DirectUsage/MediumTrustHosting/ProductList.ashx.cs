using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using MediumTrustHosting.Models;

namespace MediumTrustHosting
{
    public class ProductList : BaseHandler
    {
        public override void Process()
        {
            var repos = new ProductRepository();

            var view = CreateView("productlist.spark", "master.spark");
            view.ViewData["products"] = repos.ListAll();
            view.RenderView(Context.Response.Output);
        }
    }
}
