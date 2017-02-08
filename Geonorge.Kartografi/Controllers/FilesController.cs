using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Geonorge.Kartografi.Models;
using Geonorge.Kartografi.Services;
using System.Web;
using System;
using System.IO;
using System.Collections.Generic;

namespace Geonorge.Kartografi.Controllers
{
    public class FilesController : Controller
    {
        ICartographyService _cartographyService;
        private readonly IAuthorizationService _authorizationService;

        public FilesController(ICartographyService cartographyService, IAuthorizationService authorizationService)
        {
            _cartographyService = cartographyService;
            _authorizationService = authorizationService;
        }

        // GET: Datasets
        public ActionResult Index()
        {
            return View(_cartographyService.GetDatasets());
        }

        // GET: Files in dataset
        public ActionResult Files(string uuid = null)
        {
            return View(_cartographyService.GetCartography(uuid));
        }

        // GET: Files/Create
        public ActionResult Create()
        {
            CartographyFile cartographyFile = new CartographyFile();
            cartographyFile.OwnerPerson = _authorizationService.GetSecurityClaim("organization").FirstOrDefault();
            cartographyFile.LastEditedBy = _authorizationService.GetSecurityClaim("username").FirstOrDefault();
            ViewBag.Formats = new SelectList(CodeList.Formats, "Key", "Value", "sld");
            ViewBag.Compatibility = new SelectList(CodeList.Compatibility, "Key", "Value", string.Empty);
            ViewBag.Statuses = new SelectList(CodeList.Status, "Key", "Value", "Submitted");
            return View(cartographyFile);
        }

        // GET: Files/Details/5
        public ActionResult Details(Guid? SystemId)
        {
            if (SystemId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VersionsItem cartographyFile = _cartographyService.Versions(SystemId);
            if (cartographyFile == null)
            {
                return HttpNotFound();
            }
            return View(cartographyFile);
        }


        // POST: Files/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CartographyFile cartographyFile, HttpPostedFileBase uploadPreviewImage, HttpPostedFileBase uploadFile, string[] compatibilities)
        {
            cartographyFile.OwnerPerson = _authorizationService.GetSecurityClaim("organization").FirstOrDefault();
            cartographyFile.LastEditedBy = _authorizationService.GetSecurityClaim("username").FirstOrDefault();
            ViewBag.Formats = new SelectList(CodeList.Formats, "Key", "Value", "sld");
            ViewBag.Compatibility = new SelectList(CodeList.Compatibility, "Key", "Value", string.Empty);
            ViewBag.Statuses = new SelectList(CodeList.Status, "Key", "Value", cartographyFile.Status);
            cartographyFile.Compatibility = new List<Compatibility>();
            if(compatibilities != null)
            {
                foreach (var item in compatibilities)
                    cartographyFile.Compatibility.Add(new Compatibility {Id = Guid.NewGuid().ToString(),  Key = item });
            }

            if (ModelState.IsValid)
            {
                _cartographyService.AddCartography(cartographyFile, uploadFile, uploadPreviewImage);
                return RedirectToAction("Index", "Files", new { uuid = cartographyFile.DatasetUuid });
            }

            return View(cartographyFile);
        }

        // GET: Files/Edit/5
        public ActionResult Edit(Guid SystemId, bool newversion = false)
        {
            if (SystemId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CartographyFile cartographyFile = _cartographyService.GetCartography(SystemId);
            if (cartographyFile == null)
            {
                return HttpNotFound();
            }
            ViewBag.Formats = new SelectList(CodeList.Formats, "Key", "Value", cartographyFile.Format);
            ViewBag.newversion = newversion;
            ViewBag.compatibilitiesList = new MultiSelectList(CodeList.Compatibility, "Key", "Key", cartographyFile.Compatibility.Select(c => c.Key).ToArray());
            ViewBag.Statuses = new SelectList(CodeList.Status, "Key", "Value", cartographyFile.Status);

            return View(cartographyFile);
        }

        // POST: Files/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CartographyFile cartographyFile, HttpPostedFileBase uploadPreviewImage, HttpPostedFileBase uploadFile, string[] compatibilities, bool newversion = false)
        {
            if (cartographyFile.OfficialStatus && cartographyFile.Status == "Accepted")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            cartographyFile.Compatibility = new List<Compatibility>();
            if (compatibilities != null)
            {
                foreach (var item in compatibilities)
                    cartographyFile.Compatibility.Add(new Compatibility { Id = Guid.NewGuid().ToString(), Key = item });
            }

            ViewBag.Formats = new SelectList(CodeList.Formats, "Key", "Value", cartographyFile.Format);
            ViewBag.newversion = newversion;
            ViewBag.compatibilitiesList = new MultiSelectList(CodeList.Compatibility, "Key", "Key", cartographyFile.Compatibility.Select(c => c.Key).ToArray());
            ViewBag.Statuses = new SelectList(CodeList.Status, "Key", "Value", cartographyFile.Status);

            if (ModelState.IsValid)
            {
                if (newversion)
                {
                    CartographyFile originalCartographyFile = _cartographyService.GetCartography(cartographyFile.SystemId);
                    originalCartographyFile.Status = "Superseded";
                    _cartographyService.UpdateCartography(originalCartographyFile);
                    _cartographyService.AddCartographyVersion(cartographyFile, uploadFile, uploadPreviewImage);
                }
                else
                    _cartographyService.UpdateCartography(cartographyFile);

                return RedirectToAction("Files", "Files", new { uuid = cartographyFile.DatasetUuid });
            }
            return View(cartographyFile);
        }

        // GET: Files/Delete/5
        public ActionResult Delete(Guid? SystemId)
        {
            if (SystemId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartographyFile cartographyFile = _cartographyService.GetCartography(SystemId);
            if (cartographyFile == null)
            {
                return HttpNotFound();
            }
            return View(cartographyFile);
        }

        // POST: Files/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(Guid SystemId)
        {
            CartographyFile cartographyFile = _cartographyService.GetCartography(SystemId);
            if (cartographyFile.OfficialStatus && cartographyFile.Status == "Accepted")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            _cartographyService.RemoveCartography(cartographyFile);
            return RedirectToAction("Index");
        }

        // GET: Files/File/5
        public ActionResult File(Guid SystemId)
        {
            if (SystemId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CartographyFile cartographyFile = _cartographyService.GetCartography(SystemId);
            if (cartographyFile == null)
            {
                return HttpNotFound();
            }

            return View(cartographyFile);
        }
    }
}
