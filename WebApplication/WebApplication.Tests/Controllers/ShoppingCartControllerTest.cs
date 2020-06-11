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
using System.Collections;
using System.Web.Routing;

namespace WebApplication.Tests.Controllers
{
    public class MockHttpSession : HttpSessionStateBase
    {
        public Hashtable Buffer = new Hashtable();

        public override object this[string key]
        {
            get
            {
                return Buffer[key];
            }
            set
            {
                Buffer[key] = value;
            }
        }
    }

    [TestClass]
    public class ShoppingCartControllerTest
    {
        [TestMethod]
        public void TestIndex()
        {
            var session = new MockHttpSession();
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Session).Returns(session);

            var controller = new ShoppingCartController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            session["ShoppingCart"] = null;
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);

            var model = result.Model as List<BillDetail>;
            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Count);

            var db = new CS4PEntities();
            var product = db.Products.First();
            var shoppingCart = new List<BillDetail>();

            shoppingCart.Add(new BillDetail
            {
                Product = product,
                Quantity = 1
            });

            var billDetail = new BillDetail();
            billDetail.Product = product;
            billDetail.Quantity = 2;
            shoppingCart.Add(billDetail);

            session["ShoppingCart"] = shoppingCart;
            result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);

            model = result.Model as List<BillDetail>;
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Count);
            Assert.AreEqual(product.id, model.First().Product.id);
            Assert.AreEqual(3, model.First().Quantity);
        }

        [TestMethod]
        public void TestCreate()
        {
            var session = new MockHttpSession();
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Session).Returns(session);

            var controller = new ShoppingCartController();
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            var db = new CS4PEntities();
            var product = db.Products.First();
            var result = controller.Create(product.id, 2) as RedirectToRouteResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);

            var shoppingCart = session["ShoppingCart"] as List<BillDetail>;
            Assert.IsNotNull(shoppingCart);
            Assert.AreEqual(1, shoppingCart.Count);
            Assert.AreEqual(product.id, shoppingCart.First().Product.id);
            Assert.AreEqual(2, shoppingCart.First().Quantity);
        }
    }
}
