using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NorthwindDemo.Controllers;
using NorthwindDemo.Models;
using NorthwindModel;

namespace UnitTests.Controllers
{
    /// <summary>
    /// Summary description for HomeControllerTest
    /// </summary>
    [TestClass]
    public class ProductsControllerTest
    {
        [TestMethod]
        public void CanRenderCategories()
        {
            // Arrange
            var categories = new List<Category>();
            categories.Add(new Category { CategoryID = 42, Description = "Babelfish" });
            var repository = new Mock<NorthwindRepository>();
            repository.Setup(r => r.Categories).Returns(categories.AsQueryable());
            
            // Act
            var controller = new ProductsController(repository.Object);
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Expected the result to be a ViewResult");
            var viewData = result.ViewData.Model as IEnumerable<Category>;
            Assert.IsNotNull(viewData);
            Assert.AreEqual(1, viewData.Count());
            Assert.AreEqual("Babelfish", viewData.First().Description);
        }

        [TestMethod]
        public void CanListProductsByCategoryId()
        {
            // Arrange
            var categories = new List<Category>();
            categories.Add(new Category { CategoryID = 42, CategoryName = "Beer" });

            var products = new List<Product>();
            products.Add(new Product { CategoryID = 42, ProductName = "Heineken" });
            products.Add(new Product { CategoryID = 42, ProductName = "Guiness" });
            products.Add(new Product { CategoryID = 42, ProductName = "Hefeweisen" });

            var repository = new Mock<NorthwindRepository>();
            repository.Setup(r => r.Categories).Returns(categories.AsQueryable());
            repository.Setup(r => r.Products).Returns(products.AsQueryable());

            // Act
            var controller = new ProductsController(repository.Object);
            var result = controller.List("Beer") as ViewResult;
            
            // Assert
            Assert.IsNotNull(result, "Expected the result to be a render view result");

            var viewData = result.ViewData.Model as IEnumerable<Product>;
            Assert.AreEqual(3, viewData.Count());
            Assert.AreEqual(42, viewData.First().CategoryID);
            Assert.AreEqual("Heineken", viewData.First().ProductName);
        }

        [TestMethod]
        public void DetailRendersProduct()
        {
            // Arrange
            var products = new List<Product>();
            products.Add(new Product{ ProductID= 123, ProductName="Roomba" });

            var repository = new Mock<NorthwindRepository>();
            repository.Setup(r => r.Products).Returns(products.AsQueryable());
            
            // Act
            var controller = new ProductsController(repository.Object);
            var result = controller.Detail(123) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Expected the result to be a render view result");

            var viewData = result.ViewData.Model as Product;
            Assert.AreEqual(123, viewData.ProductID);
            Assert.AreEqual("Roomba", viewData.ProductName);
        }

        [TestMethod]
        public void CanUpdateProduct()
        {
            // Arrange
            var products = new List<Product>();
            products.Add(new Product { ProductID = 321, ProductName = "Hello Kitty Phone" });
            products[0].Category = new Category { CategoryName="Macho Stuff" };

            var repository = new Mock<NorthwindRepository>();
            repository.Setup(r => r.Products).Returns(products.AsQueryable());
            repository.Setup(r => r.SubmitChanges()).AtMostOnce();

            var formVars = new NameValueCollection();
            formVars.Add("ProductName", "Megawatt Power Tools");

            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request.Form).Returns(formVars);

            // Act
            var controller = new ProductsController(repository.Object);
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            var result = controller.Edit(321, new FormCollection()) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "Expected the result to be a redirect");
            Assert.AreEqual("List", result.RouteValues["action"]);
            Assert.AreEqual("Macho Stuff", result.RouteValues["id"].ToString());
        }

        [TestMethod]
        public void CanEditProduct()
        {
            // Arrange
            var products = new List<Product>();
            products.Add(new Product {ProductID=41, ProductName="Visual Studio"});
            var suppliers = new List<Supplier>();
            suppliers.Add(new Supplier{ CompanyName="Microsoft"});
            var categories = new List<Category>();
            categories.Add(new Category { CategoryName="Tools" });

            var repository = new Mock<NorthwindRepository>();
            repository.Setup(r => r.Products).Returns(products.AsQueryable());
            repository.Setup(r => r.Suppliers).Returns(suppliers.AsQueryable());
            repository.Setup(r => r.Categories).Returns(categories.AsQueryable());

            // Act
            var controller = new ProductsController(repository.Object);
            var result = controller.Edit(41) as ViewResult;

            // Assert
            var viewData = result.ViewData;
            var model = viewData.Model as Product;
            Assert.IsNotNull(viewData["CategoryID"]);
            Assert.IsNotNull(viewData["SupplierID"]);
            Assert.AreEqual(41, model.ProductID);
        }
    }
}
