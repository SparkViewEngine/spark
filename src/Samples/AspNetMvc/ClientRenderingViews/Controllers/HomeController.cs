// Copyright 2008 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.Linq;
using System.Web.Mvc;
using ClientRenderingViews.Models;
using Spark.Web.Mvc;

namespace ClientRenderingViews.Controllers
{
    public class HomeController : Controller
    {
        private CartRepository _cartRepos = new CartRepository();

        private ProductRepository _productRepos = new ProductRepository();

        public CartRepository CartRepos
        {
            get { return _cartRepos; }
            set { _cartRepos = value; }
        }

        public ProductRepository ProductRepos
        {
            get { return _productRepos; }
            set { _productRepos = value; }
        }

        private Cart GetCurrentCart()
        {
            Cart cart = null;
            if (Session["cartId"] != null)
                cart = CartRepos.GetCurrentCart(new Guid((string) Session["cartId"]));

            if (cart == null)
                cart = CartRepos.GetCurrentCart(Guid.Empty);

            Session["cartId"] = cart.Id.ToString("n");

            return cart;
        }

        public ActionResult Index(string ajax)
        {
            ViewData["ajaxEnabled"] = (string) Session["ajax"] == "disabled" ? false : true;
            ViewData["products"] = ProductRepos.GetProducts();
            ViewData["cart"] = GetCurrentCart();
            return View();
        }

        public ActionResult EnableAjax()
        {
            Session["ajax"] = "enabled";
            return RedirectToAction("Index");
        }

        public ActionResult DisableAjax()
        {
            Session["ajax"] = "disabled";
            return RedirectToAction("Index");
        }

        public ActionResult ShowCart()
        {
            return new JavascriptViewResult {ViewName = "_ShowCart"};
        }

        public ActionResult RefreshCart()
        {
            return Json(GetCurrentCart());
        }

        public ActionResult Reset()
        {
            Session["cartId"] = null;
            Cart cart = GetCurrentCart();

            if (Request.AcceptTypes.Contains("application/json"))
                return Json(cart);

            return RedirectToAction("index");
        }

        public ActionResult Remove(int id)
        {
            Cart cart = GetCurrentCart();
            CartItem item = cart.Items.FirstOrDefault(i => i.Product.Id == id);
            if (item != null)
                cart.Items.Remove(item);

            if (Request.AcceptTypes.Contains("application/json"))
                return Json(cart);

            return RedirectToAction("index");
        }

        public ActionResult AddToCart(int id)
        {
            Cart cart = GetCurrentCart();
            CartItem item = cart.Items.FirstOrDefault(i => i.Product.Id == id);
            if (item == null)
            {
                Product product = ProductRepos.FindProduct(id);
                cart.Items.Add(new CartItem {Product = product, Quantity = 1});
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