namespace NorthwindDemo.Controllers
{
    using System;
    using System.Web.Mvc;
    using System.Linq;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Data;
    using NorthwindModel;
    using NorthwindDemo.Models;
    using System.Web.Routing;

    public class ProductsController : Controller
    {
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
            return View(repository.Categories);
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

            return View(products.ToList());
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public object New(string id)
        {
            Product product = new Product();

            if (TempData.ContainsKey("ErrorMessage"))
            {
                foreach (var item in TempData)
                {
                    ViewData[item.Key] = item.Value;
                }
            }

            ViewData["CategoryID"] = new SelectList(repository.Categories.ToList(), "CategoryID", "CategoryName", null);
            ViewData["SupplierID"] = new SelectList(repository.Suppliers.ToList(), "SupplierID", "CompanyName", null);
            ViewData["CategoryName"] = id;

            return View(product);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public object New(string id, FormCollection form)
        {
            Product product = new Product();

            if (TryUpdateModel(product, form.ToValueProvider()) && Validate(product))
            {
                repository.InsertProductOnSubmit(product);
                repository.SubmitChanges();
                return RedirectToAction("List", new { id = product.Category.CategoryName });
            }

            ViewData["CategoryID"] = new SelectList(repository.Categories.ToList(), "CategoryID", "CategoryName", ViewData["CategoryID"]);
            ViewData["SupplierID"] = new SelectList(repository.Suppliers.ToList(), "SupplierID", "CompanyName", ViewData["SupplierID"]);

            return View(product);
        }


        public object NewCategory()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateCategory(string name, string description)
        {
            var c = new Category();
            c.CategoryName = name;
            c.Description = description;


            repository.InsertCategoryOnSubmit(c);
            repository.SubmitChanges();

            return RedirectToAction("Index");
        }

        public object Edit(int id)
        {
            Product product = repository.Products.SingleOrDefault(p => p.ProductID == id);

            if (TempData.ContainsKey("ErrorMessage"))
            {
                foreach (var item in TempData)
                {
                    ViewData[item.Key] = item.Value;
                }
            }

            ViewData["CategoryID"] = new SelectList(repository.Categories.ToList(), "CategoryID", "CategoryName", ViewData["CategoryID"] ?? product.CategoryID);
            ViewData["SupplierID"] = new SelectList(repository.Suppliers.ToList(), "SupplierID", "CompanyName", ViewData["SupplierID"] ?? product.SupplierID);

            return View(product);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public object Edit(int id, FormCollection form)
        {
            Product product = repository.Products.SingleOrDefault(p => p.ProductID == id);
            if (TryUpdateModel(product, form.ToValueProvider()) && Validate(product))
            {
                repository.SubmitChanges();
                return RedirectToAction("List", new { id = product.Category.CategoryName });
            }


            ViewData["CategoryID"] = new SelectList(repository.Categories.ToList(), "CategoryID", "CategoryName", ViewData["CategoryID"] ?? product.CategoryID);
            ViewData["SupplierID"] = new SelectList(repository.Suppliers.ToList(), "SupplierID", "CompanyName", ViewData["SupplierID"] ?? product.SupplierID);

            return View(product);
        }

        private bool Validate(Product product)
        {
            if (string.IsNullOrEmpty(product.ProductName))
                ModelState.AddModelError("ProductName", "Name is required");
            if (product.UnitPrice < 0)
                ModelState.AddModelError("UnitPrice", "Price must not be negative");
            return ModelState.IsValid;
        }

        bool IsValid()
        {
            bool valid = true;

            if (!IsValidPrice(Request.Form["UnitPrice"]))
            {
                valid = false;
                SetInvalid("UnitPrice");
            }

            if (String.IsNullOrEmpty(Request.Form["ProductName"]))
            {
                valid = false;
                SetInvalid("ProductName");
            }

            return valid;
        }

        void SetInvalid(string key)
        {
            TempData["Error:" + key] = Request.Form[key];
        }

        bool IsValidPrice(string price)
        {
            if (String.IsNullOrEmpty(price))
                return false;

            decimal result;
            return decimal.TryParse(price, out result);
        }
    }
}