using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class ShoppingCartController : Controller
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

        // GET: ShoppingCart
        public ActionResult Index()
        {
            GetShoppingCart();
            var hashtable = new Hashtable();
            foreach (var billDetail in ShoppingCart)
            {
                if (hashtable[billDetail.Product.id] != null)
                {
                    (hashtable[billDetail.Product.id] as BillDetail).Quantity += billDetail.Quantity;
                }
                else hashtable[billDetail.Product.id] = billDetail;
            }

            ShoppingCart.Clear();
            foreach (BillDetail billDetail in hashtable.Values)
                ShoppingCart.Add(billDetail);
            return View(ShoppingCart);
        }

        // GET: ShoppingCart/Create
        [HttpPost]
        public ActionResult Create(int productId, int quantity)
        {
            GetShoppingCart();
            var product = db.Products.Find(productId);
            ShoppingCart.Add(new BillDetail
            {
                Product = product,
                Quantity = quantity
            });

            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult Edit(int[] product_id, int[] quantity)
        {
            GetShoppingCart();
            ShoppingCart.Clear();
            if (product_id != null)
                for (int i=0; i<product_id.Length; i++)
                    if (quantity[i] > 0)
                    {
                        var product = db.Products.Find(product_id[i]);
                        ShoppingCart.Add(new BillDetail
                        {
                            Product = product,
                            Quantity = quantity[i]
                        });
                    }
            Session["ShoppingCart"] = ShoppingCart;

            return RedirectToAction("Index");
        }

        // GET: ShoppingCart/Delete/5
        public ActionResult Delete()
        {
            GetShoppingCart();
            ShoppingCart.Clear();
            Session["ShoppingCart"] = ShoppingCart;
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
