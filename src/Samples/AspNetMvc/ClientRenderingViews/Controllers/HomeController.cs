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
using ClientRenderingViews.Models;
using Spark.Web.Mvc;

namespace ClientRenderingViews.Controllers
{
    public class HomeController : Controller
    {
        private CartRepository _cartRepos = new CartRepository();
        public CartRepository CartRepos
        {
            get { return _cartRepos; }
            set { _cartRepos = value; }
        }

        private ProductRepository _productRepos = new ProductRepository();
        public ProductRepository ProductRepos
        {
            get { return _productRepos; }
            set { _productRepos = value; }
        }

        Cart GetCurrentCart()
        {
            Cart cart = null;
            if (Session["cartId"] != null)
                cart = CartRepos.GetCurrentCart(new Guid((string)Session["cartId"]));
            
            if (cart == null)
                cart = CartRepos.GetCurrentCart(Guid.Empty);

            Session["cartId"] = cart.Id.ToString("n");

            return cart;
        }

        public object Index(string ajax)
        {
            ViewData["ajaxEnabled"] = (string)Session["ajax"] == "disabled" ? false : true;
            ViewData["products"] = ProductRepos.GetProducts();
            ViewData["cart"] = GetCurrentCart();
            return View();
        }

        public object EnableAjax()
        {
            Session["ajax"] = "enabled";
            return RedirectToAction("Index");
        }

        public object DisableAjax()
        {
            Session["ajax"] = "disabled";
            return RedirectToAction("Index");
        }

        public object ShowCart()
        {
            return new JavascriptViewResult { ViewName = "_ShowCart" };
        }

        public object RefreshCart()
        {
            return Json(GetCurrentCart());
        }

        public object Reset()
        {
            Session["cartId"] = null;
            var cart = GetCurrentCart();

            if (Request.AcceptTypes.Contains("application/json"))
                return Json(cart);

            return RedirectToAction("index");
        }

        public object Remove(int id)
        {
            var cart = GetCurrentCart();
            var item = cart.Items.FirstOrDefault(i => i.Product.Id == id);
            if (item != null)
                cart.Items.Remove(item);

            if (Request.AcceptTypes.Contains("application/json"))
                return Json(cart);

            return RedirectToAction("index");
        }

        public object AddToCart(int id)
        {
            var cart = GetCurrentCart();
            var item = cart.Items.FirstOrDefault(i => i.Product.Id == id);
            if (item == null)
            {
                var product = ProductRepos.FindProduct(id);
                cart.Items.Add(new CartItem { Product = product, Quantity = 1 });
            }
            else
            {
                item.Quantity += 1;
            }

            if (Request.AcceptTypes.Contains("application/json"))
                return Json(cart);

            return RedirectToAction("index");
        }
    }
}
