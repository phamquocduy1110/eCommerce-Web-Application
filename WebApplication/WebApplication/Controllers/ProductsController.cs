using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private CS4PEntities db = new CS4PEntities();

        // GET: Products
        public ActionResult Index()
        {
            var model = db.Products.ToList();
            return View(model);
        }

        // for customer to view products
        [AllowAnonymous]
        public ActionResult Index2()
        {
            var model = db.Products.ToList();
            return View(model);
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [AllowAnonymous]
        public ActionResult Picture(int id)
        {
            var path = Server.MapPath(PICTURE_PATH);
            return File(path + id, "images");
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product model, HttpPostedFileBase picture)
        {
            ValidateProduct(model);
            if (ModelState.IsValid)
            {
                if (picture != null)
                {
                    using (var scope = new TransactionScope())
                    {
                        db.Products.Add(model);
                        db.SaveChanges();

                        // store picture
                        var path = Server.MapPath(PICTURE_PATH);
                        picture.SaveAs(path + model.id);

                        scope.Complete();
                        return RedirectToAction("Index");
                    }
                }
                else ModelState.AddModelError("", "Picture not found!");
            }

            return View(model);
        }

        private const string PICTURE_PATH = "~/Upload/Products/";

        private void ValidateProduct(Product product)
        {
            if (product.Price < 0)
                ModelState.AddModelError("Price", "Price is less than Zero");
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int id)
        {
            var model = db.Products.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model, HttpPostedFileBase picture)
        {
            ValidateProduct(model);
            if (ModelState.IsValid)
            {
                using (var scope = new TransactionScope())
                {
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();

                    if (picture != null)
                    {
                        var path = Server.MapPath(PICTURE_PATH);
                        picture.SaveAs(path + model.id);
                    }

                    scope.Complete();
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int id)
        {
            var model = db.Products.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var scope = new TransactionScope())
            {
                var model = db.Products.Find(id);
                db.Products.Remove(model);
                db.SaveChanges();

                var path = Server.MapPath(PICTURE_PATH);
                System.IO.File.Delete(path + model.id);

                scope.Complete();
                return RedirectToAction("Index");
            }
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
