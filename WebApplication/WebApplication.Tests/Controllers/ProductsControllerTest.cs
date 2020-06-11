using Moq;
using System;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplication.Controllers;
using WebApplication.Models;
using System.Transactions;

namespace WebApplication.Tests.Controllers
{
    [TestClass]
    public class ProductsControllerTest
    {
        [TestMethod]
        public void TestIndex()
        {
            var controller = new ProductsController();

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);

            var model = result.Model as List<Product>;
            Assert.IsNotNull(model);

            var db = new CS4PEntities();
            Assert.AreEqual(db.Products.Count(), model.Count);
        }

        [TestMethod]
        public void TestIndex2()
        {
            var controller = new ProductsController();

            var result = controller.Index2() as ViewResult;
            Assert.IsNotNull(result);

            var model = result.Model as List<Product>;
            Assert.IsNotNull(model);

            var db = new CS4PEntities();
            Assert.AreEqual(db.Products.Count(), model.Count);
        }

        [TestMethod]
        public void TestCreateG()
        {
            var controller = new ProductsController();

            var result = controller.Create() as ViewResult;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestCreateP()
        {
            var rand = new Random();
            var product = new Product
            {
                Name = rand.NextDouble().ToString(),
                Description = rand.NextDouble().ToString(),
                Price = -rand.Next()
            };

            var controller = new ProductsController();

            var result0 = controller.Create(product, null) as ViewResult;
            Assert.IsNotNull(result0);
            Assert.AreEqual("Price is less than Zero", controller.ModelState["Price"].Errors[0].ErrorMessage);

            product.Price = -product.Price;
            controller.ModelState.Clear();

            result0 = controller.Create(product, null) as ViewResult;
            Assert.IsNotNull(result0);
            Assert.AreEqual("Picture not found!", controller.ModelState[""].Errors[0].ErrorMessage);

            var picture = new Mock<HttpPostedFileBase>();
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Server).Returns(server.Object);
            controller.ControllerContext = new ControllerContext(context.Object,
                new System.Web.Routing.RouteData(), controller);

            var fileName = String.Empty;
            server.Setup(s => s.MapPath(It.IsAny<string>())).Returns<string>(s => s);
            picture.Setup(p => p.SaveAs(It.IsAny<string>())).Callback<string>(s => fileName = s);

            using (var scope = new TransactionScope())
            {
                controller.ModelState.Clear();
                var result1 = controller.Create(product, picture.Object) as RedirectToRouteResult;
                Assert.IsNotNull(result1);
                Assert.AreEqual("Index", result1.RouteValues["action"]);

                var db = new CS4PEntities();
                var entity = db.Products.SingleOrDefault(p => p.Name == product.Name && p.Description == product.Description);
                Assert.IsNotNull(entity);
                Assert.AreEqual(product.Price, entity.Price);

                Assert.IsTrue(fileName.StartsWith("~/Upload/Products/"));
                Assert.IsTrue(fileName.EndsWith(entity.id.ToString()));
            }
        }

        [TestMethod]
        public void TestEditG()
        {
            var controller = new ProductsController();
            var result0 = controller.Edit(0) as HttpNotFoundResult;
            Assert.IsNotNull(result0);

            var db = new CS4PEntities();
            var product = db.Products.First();
            var result1 = controller.Edit(product.id) as ViewResult;
            Assert.IsNotNull(result1);

            var model = result1.Model as Product;
            Assert.IsNotNull(model);
            Assert.AreEqual(product.Name, model.Name);
            Assert.AreEqual(product.Price, model.Price);
            Assert.AreEqual(product.Description, model.Description);
        }

        [TestMethod]
        public void TestEditP()
        {
            var rand = new Random();
            var db = new CS4PEntities();
            var product = db.Products.AsNoTracking().First();
            product.Name = rand.NextDouble().ToString();
            product.Description = rand.NextDouble().ToString();
            product.Price = -rand.Next();

            var controller = new ProductsController();

            var result0 = controller.Edit(product, null) as ViewResult;
            Assert.IsNotNull(result0);
            Assert.AreEqual("Price is less than Zero", controller.ModelState["Price"].Errors[0].ErrorMessage);

            var picture = new Mock<HttpPostedFileBase>();
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Server).Returns(server.Object);
            controller.ControllerContext = new ControllerContext(context.Object,
                new System.Web.Routing.RouteData(), controller);

            var fileName = String.Empty;
            server.Setup(s => s.MapPath(It.IsAny<string>())).Returns<string>(s => s);
            picture.Setup(p => p.SaveAs(It.IsAny<string>())).Callback<string>(s => fileName = s);

            using (var scope = new TransactionScope())
            {
                product.Price = -product.Price;
                controller.ModelState.Clear();
                var result1 = controller.Edit(product, picture.Object) as RedirectToRouteResult;
                Assert.IsNotNull(result1);
                Assert.AreEqual("Index", result1.RouteValues["action"]);

                var entity = db.Products.Find(product.id);
                Assert.IsNotNull(entity);
                Assert.AreEqual(product.Name, entity.Name);
                Assert.AreEqual(product.Description, entity.Description);
                Assert.AreEqual(product.Price, entity.Price);

                Assert.AreEqual("~/Upload/Products/" + product.id, fileName);
                //Assert.IsTrue(fileName.StartsWith("~/Upload/Products/"));
                //Assert.IsTrue(fileName.EndsWith(entity.id.ToString()));
            }
        }

        [TestMethod]
        public void TestDeleteG()
        {
            var controller = new ProductsController();
            var result0 = controller.Delete(0) as HttpNotFoundResult;
            Assert.IsNotNull(result0);

            var db = new CS4PEntities();
            var product = db.Products.First();
            var result1 = controller.Delete(product.id) as ViewResult;
            Assert.IsNotNull(result1);

            var model = result1.Model as Product;
            Assert.IsNotNull(model);
            Assert.AreEqual(product.Name, model.Name);
            Assert.AreEqual(product.Price, model.Price);
            Assert.AreEqual(product.Description, model.Description);
        }

        [TestMethod]
        public void TestDeleteP()
        {
            var db = new CS4PEntities();
            var product = db.Products.AsNoTracking().First();

            var controller = new ProductsController();

            var context = new Mock<HttpContextBase>();
            var server = new Mock<HttpServerUtilityBase>();
            context.Setup(c => c.Server).Returns(server.Object);
            controller.ControllerContext = new ControllerContext(context.Object,
                new System.Web.Routing.RouteData(), controller);

            var filePath = String.Empty;
            var tempDir = System.IO.Path.GetTempFileName();
            server.Setup(s => s.MapPath(It.IsAny<string>())).Returns<string>(s =>
            {
                filePath = s;
                return tempDir;
            });

            using (var scope = new TransactionScope())
            {
                System.IO.File.Delete(tempDir);
                System.IO.Directory.CreateDirectory(tempDir);
                tempDir = tempDir + "/";
                System.IO.File.Create(tempDir + product.id).Close();
                Assert.IsTrue(System.IO.File.Exists(tempDir + product.id));

                var result = controller.DeleteConfirmed(product.id) as RedirectToRouteResult;
                Assert.IsNotNull(result);
                Assert.AreEqual("Index", result.RouteValues["action"]);

                var entity = db.Products.Find(product.id);
                Assert.IsNull(entity);

                Assert.AreEqual("~/Upload/Products/", filePath);
                Assert.IsFalse(System.IO.File.Exists(tempDir + product.id));
            }
        }

        [TestMethod]
        public void TestPicture()
        {
            var controller = new ProductsController();

            var context = new Mock<HttpContextBase>();
            var server = new Mock<HttpServerUtilityBase>();
            context.Setup(c => c.Server).Returns(server.Object);
            controller.ControllerContext = new ControllerContext(context.Object,
                new System.Web.Routing.RouteData(), controller);

            var filePath = String.Empty;
            server.Setup(s => s.MapPath(It.IsAny<string>())).Returns<string>(s => filePath = s);

            var result = controller.Picture(0) as FilePathResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("~/Upload/Products/0", result.FileName);
            Assert.AreEqual("images", result.ContentType);
        }

        [TestMethod]
        public void TestDispose()
        {
            using (var controller = new ProductsController()) { }
        }

        [TestMethod]
        public void TestDetails()
        {
            var controller = new ProductsController();
            var result0 = controller.Details(0) as HttpNotFoundResult;
            Assert.IsNotNull(result0);

            var db = new CS4PEntities();
            var product = db.Products.First();
            var result1 = controller.Details(product.id) as ViewResult;
            Assert.IsNotNull(result1);

            var model = result1.Model as Product;
            Assert.IsNotNull(model);
            Assert.AreEqual(product.Name, model.Name);
            Assert.AreEqual(product.Price, model.Price);
            Assert.AreEqual(product.Description, model.Description);
        }

        [TestMethod]
        public void TestSearch()
        {
            var db = new CS4PEntities();
            var products = db.Products.ToList();
            var keyword = products.First().Name.Split().First();
            products = products.Where(p => p.Name.ToLower().Contains(keyword.ToLower())).ToList();

            var controller = new ProductsController();
            var result = controller.Search(keyword) as ViewResult;
            Assert.IsNotNull(result);

            var model = result.Model as List<Product>;
            Assert.IsNotNull(model);

            Assert.AreEqual("Index2", result.ViewName);
            Assert.AreEqual(products.Count(), model.Count);
            Assert.AreEqual(keyword, result.ViewData["Keyword"]);
        }
    }
}
