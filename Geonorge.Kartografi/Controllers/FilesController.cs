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
using PagedList;
using log4net;
using System.Collections;
using Geonorge.Kartografi.Helpers;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;
using System.Web.Configuration;
using Microsoft.Owin.Security.Cookies;
using System.Security.Claims;
using Geonorge.AuthLib.Common;

namespace Geonorge.Kartografi.Controllers
{
    [HandleError]
    public class FilesController : Controller
    {
        ICartographyService _cartographyService;
        private readonly IAuthorizationService _authorizationService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        public FilesController(ICartographyService cartographyService, IAuthorizationService authorizationService)
        {
            _cartographyService = cartographyService;
            _authorizationService = authorizationService;
        }

        // GET: Datasets
        public ActionResult Index(string sortOrder, string text, int? page, bool limitofficial = false, string ownerdataset = "")
        {
            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "datasetname";

            ViewBag.DatasetNameSortParm = sortOrder == "datasetname" ? "datasetname_desc" : "datasetname";
            ViewBag.DatasetOwner = sortOrder == "datasetowner" ? "datasetowner_desc" : "datasetowner";
            ViewBag.Theme = sortOrder == "theme" ? "theme_desc" : "theme";
            ViewBag.SortOrder = sortOrder;
            ViewBag.text = text;
            ViewBag.limitofficial = limitofficial;
            ViewBag.Page = 0;
            ViewBag.ownerdataset = ownerdataset;

            return View();
        }

        // GET: Files in dataset
        public ActionResult Files(string uuid = null)
        {
            return View(_cartographyService.GetCartography(uuid).OrderBy(s => s.Name));
        }

        // GET: Files/Create
        public ActionResult Create(string uuid = null)
        {
            CartographyFile file = new CartographyFile();

            if (!string.IsNullOrEmpty(uuid))
            {
                var data = _cartographyService.GetCartography(uuid).FirstOrDefault();
                if(data != null)
                { 
                    file.DatasetUuid = data.DatasetUuid;
                    file.DatasetName = data.DatasetName;
                    file.Theme = data.Theme;
                    file.OwnerDataset = data.OwnerDataset;
                }
            }
            file.AddMissingTranslations();
            ViewBag.IsAdmin = false;
            if (Request.IsAuthenticated)
            {
                ViewBag.IsAdmin = _authorizationService.IsAdmin();
            }

            ViewBag.Compatibility = new SelectList(CodeList.Compatibility, "Key", "Value", string.Empty);
            ViewBag.Statuses = new SelectList(GetStatusTranslated(CodeList.Status), "Key", "Value", "Submitted");
            return View(file);
        }

        private IEnumerable GetStatusTranslated(Dictionary<string, string> status)
        {
            if (CultureHelper.IsNorwegian())
                return status;
            else
            {
                Dictionary<string, string> statusTranslated = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> kvp in status)
                {
                    statusTranslated[kvp.Key] = kvp.Key;
                }

                return statusTranslated;
            }
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

            ViewBag.HasAccess = false;
            ViewBag.IsAdmin = false;
            if (Request.IsAuthenticated)
            {
                ViewBag.HasAccess = _authorizationService.HasAccess(cartographyFile.CurrentVersion.Owner,
                    ClaimsPrincipal.Current.GetOrganizationName());
                ViewBag.IsAdmin = _authorizationService.IsAdmin();
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
            if (uploadPreviewImage != null)
            {
                if (!(uploadPreviewImage.ContentType == "image/jpeg" || uploadPreviewImage.ContentType == "image/gif" 
                    || uploadPreviewImage.ContentType == "image/png" || uploadPreviewImage.ContentType == "image/svg+xml"))
                {
                    ModelState.AddModelError(string.Empty, "Miniatyrbilde må være bilde format");
                }
            }

            ViewBag.IsAdmin = false;
            if (Request.IsAuthenticated)
            {
                ViewBag.IsAdmin = _authorizationService.IsAdmin();
            }

            if (!ViewBag.IsAdmin && cartographyFile.Status == "Accepted")
            {
                cartographyFile.DateAccepted = null;
                cartographyFile.AcceptedComment = "";
                cartographyFile.Status = "Submitted";
                ModelState.Remove("DateAccepted");
                ModelState.Remove("AcceptedComment");
                ModelState.Remove("Status");
                ModelState.AddModelError(string.Empty, "Kun administrator kan godkjenne");
            }

            ViewBag.Compatibility = new SelectList(CodeList.Compatibility, "Key", "Value", string.Empty);
            ViewBag.Statuses = new SelectList(GetStatusTranslated(CodeList.Status), "Key", "Value", cartographyFile.Status);
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

            ViewBag.newversion = newversion;
            ViewBag.compatibilitiesList = new MultiSelectList(CodeList.Compatibility, "Key", "Key", cartographyFile.Compatibility.Select(c => c.Key).ToArray());
            ViewBag.Statuses = new SelectList(GetStatusTranslated(CodeList.Status), "Key", "Value", cartographyFile.Status);

            ViewBag.IsAdmin = false;
            if (Request.IsAuthenticated)
            {
                ViewBag.IsAdmin = _authorizationService.IsAdmin();
            }

            return View(cartographyFile);
        }

        // POST: Files/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CartographyFile cartographyFile, HttpPostedFileBase uploadPreviewImage, HttpPostedFileBase uploadFile, string[] compatibilities, bool newversion = false)
        {

            if (uploadPreviewImage != null)
            {
                if (!(uploadPreviewImage.ContentType == "image/jpeg" || uploadPreviewImage.ContentType == "image/gif"
                    || uploadPreviewImage.ContentType == "image/png" || uploadPreviewImage.ContentType == "image/svg+xml"))
                {
                    ModelState.AddModelError(string.Empty, "Miniatyrbilde må være bilde format");
                }
            }

            CartographyFile originalCartographyFile = _cartographyService.GetCartography(cartographyFile.SystemId);

            ViewBag.IsAdmin = _authorizationService.IsAdmin();
   
            if (!_authorizationService.HasAccess(originalCartographyFile.Owner,
                    ClaimsPrincipal.Current.GetOrganizationName()))
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            if (!ViewBag.IsAdmin && cartographyFile.Status == "Accepted")
            {
                cartographyFile.DateAccepted = null;
                cartographyFile.AcceptedComment = "";
                cartographyFile.Status = "Submitted";
                ModelState.Remove("DateAccepted");
                ModelState.Remove("AcceptedComment");
                ModelState.Remove("Status");
                ModelState.AddModelError(string.Empty, "Kun administrator kan godkjenne");
            }

            cartographyFile.Compatibility = new List<Compatibility>();
            if (compatibilities != null)
            {
                foreach (var item in compatibilities)
                    cartographyFile.Compatibility.Add(new Compatibility { Id = Guid.NewGuid().ToString(), Key = item });
            }

            ViewBag.newversion = newversion;
            ViewBag.compatibilitiesList = new MultiSelectList(CodeList.Compatibility, "Key", "Key", cartographyFile.Compatibility.Select(c => c.Key).ToArray());
            ViewBag.Statuses = new SelectList(GetStatusTranslated(CodeList.Status), "Key", "Value", cartographyFile.Status);

            if (ModelState.IsValid)
            {
                if (newversion)
                {
                    originalCartographyFile.Status = "Superseded";
                    _cartographyService.UpdateCartography(originalCartographyFile);
                    _cartographyService.AddCartographyVersion(cartographyFile, uploadFile, uploadPreviewImage);
                }
                else
                    _cartographyService.UpdateCartography(originalCartographyFile, cartographyFile, uploadPreviewImage);

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

            bool hasAccess = _authorizationService.HasAccess(cartographyFile.Owner,
                ClaimsPrincipal.Current.GetOrganizationName());

            bool isAdmin = _authorizationService.IsAdmin();

            if (!hasAccess)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            if (cartographyFile.OfficialStatus && cartographyFile.Status == "Accepted" && !isAdmin)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var cartography = _cartographyService.RemoveCartography(cartographyFile);

            if (cartography != null)
            {
                var versions =_cartographyService.Versions(cartography.SystemId);

                if (versions != null && versions.CurrentVersion != null)
                {
                    return RedirectToAction("Details", "Files", new { systemid = versions.CurrentVersion.SystemId });
                }
                else
                    return RedirectToAction("Index");
            }
            else
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

            ViewBag.HasAccess = false;
            ViewBag.IsAdmin = false;
            if (Request.IsAuthenticated)
            {
                ViewBag.HasAccess = _authorizationService.HasAccess(cartographyFile.Owner,
                    ClaimsPrincipal.Current.GetOrganizationName());
                ViewBag.IsAdmin = _authorizationService.IsAdmin();
            }

            return View(cartographyFile);
        }

        public FileResult Download(Guid? systemid)
        {

            var file = _cartographyService.GetCartography(systemid.Value);
            string targetFolder = System.Web.HttpContext.Current.Server.MapPath("~/files/");

            byte[] fileBytes = System.IO.File.ReadAllBytes(@""+ targetFolder + file.FileName);

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, file.FileName);
        }

        public ActionResult CartographyList(int page, string sortOrder, string text, bool limitofficial = false, string ownerdataset = "" )
        {
            var datasets = _cartographyService.GetDatasets(text, limitofficial, "", ownerdataset);
            switch (sortOrder)
            {
                case "datasetname_desc":
                    datasets = datasets.OrderByDescending(s => s.DatasetName).ToList();
                    break;
                case "datasetowner":
                    datasets = datasets.OrderBy(s => s.OwnerDataset).ToList();
                    break;
                case "datasetowner_desc":
                    datasets = datasets.OrderByDescending(s => s.OwnerDataset).ToList();
                    break;
                case "theme_desc":
                    datasets = datasets.OrderByDescending(s => s.Theme).ToList();
                    break;
                case "theme":
                    datasets = datasets.OrderBy(s => s.Theme).ToList();
                    break;
                default:
                    datasets = datasets.OrderBy(s => s.DatasetName).ToList();
                    break;
            }
            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "datasetname";

            ViewBag.DatasetNameSortParm = sortOrder == "datasetname" ? "datasetname_desc" : "datasetname";
            ViewBag.DatasetOwner = sortOrder == "datasetowner" ? "datasetowner_desc" : "datasetowner";
            ViewBag.Theme = sortOrder == "theme" ? "theme_desc" : "theme";
            ViewBag.SortOrder = sortOrder;
            ViewBag.text = text;
            ViewBag.limitofficial = limitofficial;
            ViewBag.ownerdataset = ownerdataset;

            int rangeStart = 0;
            int rangeLength = 0;

            int pageSize = 30;
            int pageNumber = page;

            if ((pageNumber * pageSize) > datasets.Count)
                return new EmptyResult();

            if (((pageNumber * pageSize) + pageSize) > datasets.Count)
            {
                rangeLength = datasets.Count % pageSize;
                rangeStart = datasets.Count - rangeLength;
            }
            else
            {
                rangeStart = (pageNumber * pageSize);
                rangeLength = pageSize;
            }

            datasets = datasets.GetRange(rangeStart, rangeLength);

            ViewBag.Page = pageNumber + 1;
            return PartialView("_CartographyList", datasets);
        }


        public void SignIn()
        {
            var redirectUrl = Url.Action(nameof(FilesController.Index), "Files");

            if (Request.QueryString["ReturnUrl"] != null)
            {
                redirectUrl = Request.QueryString["ReturnUrl"];
            }

            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public void SignOut()
        {
            var redirectUri = WebConfigurationManager.AppSettings["GeoID:PostLogoutRedirectUri"];
            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = redirectUri },
                OpenIdConnectAuthenticationDefaults.AuthenticationType,
                CookieAuthenticationDefaults.AuthenticationType);
        }

        /// <summary>
        /// This is the action responding to /signout-callback-oidc route after logout at the identity provider
        /// </summary>
        /// <returns></returns>
        public ActionResult SignOutCallback()
        {
            var redirect = VirtualPathUtility.ToAbsolute("~/Files/Index") + "?logout=true";
            return Redirect(redirect);
        }


        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
    }
}
