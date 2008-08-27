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
    
    public class Product : BaseHandler
    {
        public override void Process()
        {
            int id = Convert.ToInt32(Context.Request.Params["id"]);

            var repos = new ProductRepository();

            var view = CreateView("product.spark", "master.spark");
            view.ViewData["product"] = repos.Fetch(id);
            view.RenderView(Context.Response.Output);
        }
    }
}
