using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class BillController : Controller
    {
        private CS4PEntities db = new CS4PEntities();

        private List<BillDetail> ShoppingCart = null;

        private void GetShoppingCart()
        {
            if (Session["ShoppingCart"] != null)
                ShoppingCart = Session["ShoppingCart"] as List<BillDetail>;
            else
            {
                ShoppingCart = new List<BillDetail>();
                Session["ShoppingCart"] = ShoppingCart;
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var model = db.Bills.ToList();
            return View(model);
        }

        public ActionResult Create()
        {
            GetShoppingCart();
            ViewBag.Cart = ShoppingCart;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Bill model)
        {
            ValidateBill(model);
            if (ModelState.IsValid)
            {
                using (var scope = new TransactionScope())
                {
                    model.Date = DateTime.Now;
                    db.Bills.Add(model);
                    db.SaveChanges();

                    foreach (var item in ShoppingCart)
                    {
                        db.BillDetails.Add(new BillDetail
                        {
                            Bill_id = model.id,
                            Product_id = item.Product.id,
                            Price = item.Product.Price,
                            Quantity = item.Quantity
                        });
                    }
                    db.SaveChanges();

                    scope.Complete();
                    Session["ShoppingCart"] = null;
                    return RedirectToAction("Index2", "Products");
                }
            }
            GetShoppingCart();
            ViewBag.Cart = ShoppingCart;
            return View(model);
        }

        private void ValidateBill(Bill model)
        {
            var regex = new Regex("[0-9]{3}");
            GetShoppingCart();
            if (ShoppingCart.Count == 0)
                ModelState.AddModelError("", "There is no item in shopping cart!");
            if (!regex.IsMatch(model.Phone))
                ModelState.AddModelError("Phone", "Wrong phone number");
        }
    }
}