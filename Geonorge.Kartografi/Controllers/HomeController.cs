using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Geonorge.Kartografi.Helpers;

namespace Geonorge.Kartografi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Route("setculture/{culture}")]
        public ActionResult SetCulture(string culture, string returnUrl)
        {
            // Validate input
            culture = CultureHelper.GetImplementedCulture(culture);
            // Save culture in a cookie
            HttpCookie cookie = Request.Cookies["_culture"];

            if (cookie != null)
            {
                if (cookie.Domain != ".geonorge.no")
                {
                    HttpCookie oldCookie = new HttpCookie("_culture");
                    oldCookie.Domain = cookie.Domain;
                    oldCookie.Expires = DateTime.Now.AddDays(-1d);
                    Response.Cookies.Add(oldCookie);
                }
            }

            if (cookie != null)
            {
                cookie.Value = culture;   // update cookie value
                cookie.Expires = DateTime.Now.AddYears(1);
                //cookie.SameSite = SameSiteMode.Lax;
                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            else
            {
                cookie = new HttpCookie("_culture");
                cookie.Value = culture;
                cookie.Expires = DateTime.Now.AddYears(1);
                //cookie.SameSite = SameSiteMode.Lax;

                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            Response.Cookies.Add(cookie);

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index");
        }
    }
}