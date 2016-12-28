using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Geonorge.Kartografi.Models;
using Geonorge.Kartografi.Services;

namespace Geonorge.Kartografi.Controllers
{
    public class FilesController : Controller
    {
        ICartographyService _cartographyService;

        public FilesController(ICartographyService cartographyService)
        {
            _cartographyService = cartographyService;
        }

        // GET: Files
        public ActionResult Index()
        {
            return View(_cartographyService.GetCartography());
        }

        // GET: Files/Create
        public ActionResult Create()
        {
            return View();
        }

        // GET: Files/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartographyFile cartographyFile = _cartographyService.GetCartography(id);
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
        public ActionResult Create([Bind(Include = "Id,Name,Description,OwnerOrganization,OwnerPerson,LastEditedBy,FileName,Format,Use,DatasetUuid,DatasetName,ServiceUuid,ServiceName,PreviewImage,VersionId,DateChanged,Status,DateAccepted,AcceptedComment,OfficialStatus,Properties,Theme")] CartographyFile cartographyFile)
        {
            if (ModelState.IsValid)
            {
                _cartographyService.AddCartography(cartographyFile);
                return RedirectToAction("Index");
            }

            return View(cartographyFile);
        }

        // GET: Files/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartographyFile cartographyFile = _cartographyService.GetCartography(id);
            if (cartographyFile == null)
            {
                return HttpNotFound();
            }
            return View(cartographyFile);
        }

        // POST: Files/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,OwnerOrganization,OwnerPerson,LastEditedBy,FileName,Format,Use,DatasetUuid,DatasetName,ServiceUuid,ServiceName,PreviewImage,VersionId,DateChanged,Status,DateAccepted,AcceptedComment,OfficialStatus,Properties,Theme")] CartographyFile cartographyFile)
        {
            if (ModelState.IsValid)
            {
                _cartographyService.UpdateCartography(cartographyFile);
                return RedirectToAction("Index");
            }
            return View(cartographyFile);
        }

        // GET: Files/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartographyFile cartographyFile = _cartographyService.GetCartography(id);
            if (cartographyFile == null)
            {
                return HttpNotFound();
            }
            return View(cartographyFile);
        }

        // POST: Files/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CartographyFile cartographyFile = _cartographyService.GetCartography(id);
            _cartographyService.RemoveCartography(cartographyFile);
            return RedirectToAction("Index");
        }
    }
}
