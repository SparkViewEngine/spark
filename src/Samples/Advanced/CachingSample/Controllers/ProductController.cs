using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CachingSample.Models;
using Spark;

namespace CachingSample.Controllers {
    public class ProductController : Controller {
        private readonly IEnumerable<Product> _products = new[]
        {
            new Product{Id=1, Name="Alpha", Price=42},
            new Product{Id=2, Name="Beta", Price=3},
            new Product{Id=3, Name="Gamma", Price=827},
            new Product{Id=4, Name="Delta", Price=73},
            new Product{Id=5, Name="Epsilon", Price=6655321},
        };

        public ActionResult Index() {
            return View(_products);
        }

        public ActionResult Show(int id) {

            var productHolder = ValueHolder.For(id, () => {
                Response.Write("FETCHING PRODUCT #" + id);
                return _products.SingleOrDefault(p => p.Id == id);
            });
            return View(productHolder);
        }


    }
}
