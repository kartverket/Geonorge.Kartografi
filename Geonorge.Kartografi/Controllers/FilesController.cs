﻿using System.Data.Entity;
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

        public FilesController(ICartographyService cartographyService)
        {
            _cartographyService = cartographyService;
        }

        // GET: Datasets
        public ActionResult Index()
        {
            return View(_cartographyService.GetDatasets());
        }

        // GET: Files in dataset
        public ActionResult Files()
        {
            return View(_cartographyService.GetCartography());
        }

        // GET: Files/Create
        public ActionResult Create()
        {
            ViewBag.Formats = new SelectList(CodeList.Formats, "Key", "Value", "sld");
            ViewBag.Compatibility = new SelectList(CodeList.Compatibility, "Key", "Value", string.Empty);
            ViewBag.Statuses = new SelectList(CodeList.Status, "Key", "Value", "Submitted");
            return View();
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CartographyFile cartographyFile, HttpPostedFileBase uploadPreviewImage, HttpPostedFileBase uploadFile, string[] compatibilities)
        {
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
                cartographyFile.PreviewImage = SaveFile(uploadPreviewImage);
                cartographyFile.FileName = SaveFile(uploadFile);
                _cartographyService.AddCartography(cartographyFile);
                return RedirectToAction("Index");
            }

            return View(cartographyFile);
        }

        private string SaveFile(HttpPostedFileBase file)
        {
            string fileName = null;

            if(file != null)
            { 
                fileName = file.FileName;
                string targetFolder = System.Web.HttpContext.Current.Server.MapPath("~/files");
                string targetPath = Path.Combine(targetFolder, fileName);
                file.SaveAs(targetPath);
            }

            return fileName;
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
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CartographyFile cartographyFile, HttpPostedFileBase uploadPreviewImage, HttpPostedFileBase uploadFile, string[] compatibilities, bool newversion = false)
        {
            cartographyFile.Compatibility = new List<Compatibility>();
            if (compatibilities != null)
            {
                foreach (var item in compatibilities)
                    cartographyFile.Compatibility.Add(new Compatibility { Id = Guid.NewGuid().ToString(), Key = item });
            }

            var previewImage = SaveFile(uploadPreviewImage);
            var fileName = SaveFile(uploadFile);

            if(fileName != null)
            {
                newversion = true;
                CartographyFile originalCartographyFile = _cartographyService.GetCartography(cartographyFile.SystemId);
                originalCartographyFile.Status = "Superseded";
                _cartographyService.UpdateCartography(originalCartographyFile);
                cartographyFile.Status = "Submitted";
            }

            if (ModelState.IsValid)
            {
                if(newversion)
                    _cartographyService.AddCartographyVersion(cartographyFile);
                else
                    _cartographyService.UpdateCartography(cartographyFile);

                return RedirectToAction("Index");
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
        public ActionResult DeleteConfirmed(Guid SystemId)
        {
            CartographyFile cartographyFile = _cartographyService.GetCartography(SystemId);
            _cartographyService.RemoveCartography(cartographyFile);
            return RedirectToAction("Index");
        }
    }
}
