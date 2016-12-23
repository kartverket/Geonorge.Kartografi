using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Geonorge.Kartografi.Models;

namespace Geonorge.Kartografi.Controllers
{
    public class SymbolsController : Controller
    {
        private KartografiDbContext db = new KartografiDbContext();

        // GET: Symbols
        public ActionResult Index()
        {
            return View(db.Symbols.ToList());
        }

        // GET: Symbols/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Symbol symbol = db.Symbols.Find(id);
            if (symbol == null)
            {
                return HttpNotFound();
            }
            return View(symbol);
        }

        // GET: Symbols/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Symbols/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,EksternalSymbolID,OwnerOrganization,OwnerPerson,LastEditedBy,Type,DateChanged,Status,DateAccepted,OfficialStatus,Theme,Source,SourceUrl")] Symbol symbol)
        {
            if (ModelState.IsValid)
            {
                db.Symbols.Add(symbol);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(symbol);
        }

        // GET: Symbols/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Symbol symbol = db.Symbols.Find(id);
            if (symbol == null)
            {
                return HttpNotFound();
            }
            return View(symbol);
        }

        // POST: Symbols/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,EksternalSymbolID,OwnerOrganization,OwnerPerson,LastEditedBy,Type,DateChanged,Status,DateAccepted,OfficialStatus,Theme,Source,SourceUrl")] Symbol symbol)
        {
            if (ModelState.IsValid)
            {
                db.Entry(symbol).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(symbol);
        }

        // GET: Symbols/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Symbol symbol = db.Symbols.Find(id);
            if (symbol == null)
            {
                return HttpNotFound();
            }
            return View(symbol);
        }

        // POST: Symbols/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Symbol symbol = db.Symbols.Find(id);
            db.Symbols.Remove(symbol);
            db.SaveChanges();
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
