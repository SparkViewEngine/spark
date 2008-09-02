namespace NorthwindDemo.Controllers
{
    using System;
    using System.Web.Mvc;
    using System.Linq;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Data;
    using NorthwindDemo.Models;
    using System.Web.Routing;

    public class ProductsController : Controller {
        public ProductsController()
            : this(new NorthwindRepository(new NorthwindDataContext()))
        { }

        public ProductsController(NorthwindRepository context)
        {            
            this.repository = context;
        }

        NorthwindRepository repository;

        public object Index()
        {
            return Categories();
        }

        public object Categories()
        {
            return View("Categories", repository.Categories.ToList());
        }

        public object Detail(int id)
        {
            Product product = this.repository.Products.SingleOrDefault(p => p.ProductID == id);
            return View(product);
        }

        public object List(string id)
        {
            var category = repository.Categories.SingleOrDefault(c => c.CategoryName == id);

            var products = from p in repository.Products
                           where p.CategoryID == category.CategoryID
                           select p;

            ViewData["Title"] = "Hello World!";
            ViewData["CategoryName"] = id;

            //this.ViewEngine = new MvcContrib.NHamlViewEngine.NHamlViewFactory();
            return View("ListingByCategory", products.ToList());
        }

        public object Category(int id)
        {
            Category category = repository.Categories.SingleOrDefault(c => c.CategoryID == id);
            return View("List", category);
        }

        public object New()
        {
            ProductsNewViewData viewData = new ProductsNewViewData();

            viewData.Suppliers = repository.Suppliers.ToList();
            viewData.Categories = repository.Categories.ToList();

            return View("New", viewData);
        }

        public object Create()
        {
            Product product = new Product();

            throw new NotImplementedException("Not sure what BindingHelperExtensions turned into");
            //BindingHelperExtensions.UpdateFrom(product, Request.Form);

            repository.InsertProductOnSubmit(product);
            repository.SubmitChanges();

            return RedirectToRoute(new RouteValueDictionary(new { Action = "List", ID = product.Category.CategoryName }));
        }

        public object Edit(int id)
        {
            ProductsEditViewData viewData = new ProductsEditViewData();

            Product product = repository.Products.SingleOrDefault(p => p.ProductID == id);
            viewData.Product = product;

            if (TempData.ContainsKey("ErrorMessage")) {
                foreach (var item in TempData) {
                    ViewData[item.Key] = item.Value;
                }
            }

            ViewData["CategoryID"] = new SelectList(repository.Categories.ToList(), "CategoryID", "CategoryName", ViewData["CategoryID"] ?? product.CategoryID);
            ViewData["SupplierID"]= new SelectList(repository.Suppliers.ToList(), "SupplierID", "CompanyName", ViewData["SupplierID"] ?? product.SupplierID);

            return View("Edit", viewData);
        }

        public object Update(int id)
        {
            Product product = repository.Products.SingleOrDefault(p => p.ProductID == id);
            if(!IsValid())
            {
                Request.Form.CopyTo(TempData);
                TempData["ErrorMessage"] = "An error occurred";
                return RedirectToAction("Edit", new { id = id });
            }

            throw new NotImplementedException("Not sure what BindingHelperExtensions turned into");
            //BindingHelperExtensions.UpdateFrom(product, Request.Form);
            repository.SubmitChanges();

            return RedirectToRoute(new RouteValueDictionary(new { Action = "List", ID = product.Category.CategoryName }));
        }

        bool IsValid() {
            bool valid = true;
            
            if (!IsValidPrice(Request.Form["UnitPrice"])) {
                valid = false;
                SetInvalid("UnitPrice");
            }

            if (String.IsNullOrEmpty(Request.Form["ProductName"])) {
                valid = false;
                SetInvalid("ProductName");
            }

            return valid;
        }

        void SetInvalid(string key) {
            TempData["Error:" + key] = Request.Form[key];
        }

        bool IsValidPrice(string price) {
            if (String.IsNullOrEmpty(price))
                return false;

            decimal result;
            return decimal.TryParse(price, out result);
        }
    }
}