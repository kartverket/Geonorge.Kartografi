﻿using Autofac;
using Geonorge.Kartografi.App_Start;
using log4net;
using System;
using System.Globalization;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Geonorge.Kartografi.Models.Translations;
using Geonorge.Kartografi.Helpers;

namespace Geonorge.Kartografi
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        protected void Application_Start()
        {
            MvcHandler.DisableMvcResponseHeader = true;
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            DependencyConfig.Configure(new ContainerBuilder());

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;


            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("json", "true", "application/json"));
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(new UriPathExtensionMapping("json", "application/json"));

            // init log4net
            log4net.Config.XmlConfigurator.Configure();

            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError().GetBaseException();

            Log.Error("App_Error", ex);
        }

        protected void Application_BeginRequest()
        {
            var cookie = Context.Request.Cookies["_culture"];
            var userAgent = Context.Request.UserAgent;

            if ((userAgent != null && !userAgent.StartsWith("Mozilla") || userAgent == null))
                cookie = null;

            var lang = Context.Request.QueryString["lang"];
            if (!string.IsNullOrEmpty(lang))
                cookie = null;

            if (cookie == null)
            {
                var cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ?
                    Request.UserLanguages[0] : null;

                if (!string.IsNullOrEmpty(lang))
                    cultureName = lang;

                cultureName = CultureHelper.GetImplementedCulture(cultureName);
                if (CultureHelper.IsNorwegian(cultureName))
                    cookie = new HttpCookie("_culture", Culture.NorwegianCode);
                else
                    cookie = new HttpCookie("_culture", Culture.EnglishCode);

                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
                cookie.Expires = DateTime.Now.AddYears(1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }

            if (!string.IsNullOrEmpty(cookie.Value))
            {
                var culture = new CultureInfo(cookie.Value);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }
    }
}
