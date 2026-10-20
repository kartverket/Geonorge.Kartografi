using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Autofac;
using Autofac.Integration.Owin;
using Geonorge.AuthLib.Common;
using Geonorge.Kartografi.App_Start;
using log4net;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(Geonorge.Kartografi.Startup))]

namespace Geonorge.Kartografi
{
    public class Startup
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Startup));
        private const string NonceCookiePrefix = "OpenIdConnect.nonce.";

        public void Configuration(IAppBuilder app)
        {
            app.Use((context, next) => {
                context.Request.Scheme = "https";
                return next();
            });

            // Use Autofac as an Owin middleware
            var container = DependencyConfig.Configure(new ContainerBuilder());
            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();  // requires Autofac.Mvc5.Owin nuget package installed

            ConfigureAuthentication(app);
        }

        // Same setup as Geonorge.AuthLib.NetFull UseGeonorgeAuthentication(), but with System.Web cookie managers
        // (Katana and System.Web otherwise overwrite each other's Set-Cookie headers, losing auth/nonce cookies)
        // and cleanup of nonce cookies left behind by unfinished logins.
        private static void ConfigureAuthentication(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                // Other Geonorge apps on the same host (e.g. register.geonorge.no) use the default ".AspNet.Cookies" with path "/".
                // Sharing the name makes the apps overwrite each other's auth cookie, logging the user out and triggering new logins.
                CookieName = ".AspNet.Kartografi",
                CookieManager = new SystemWebChunkingCookieManager()
            });

            var cookieManager = new SystemWebCookieManager();
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = ConfigurationManager.AppSettings["GeoID:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["GeoID:ClientSecret"],
                MetadataAddress = ConfigurationManager.AppSettings["GeoID:MetadataAddress"],
                Authority = ConfigurationManager.AppSettings["GeoID:Authority"],
                RedirectUri = ConfigurationManager.AppSettings["GeoID:RedirectUri"],
                PostLogoutRedirectUri = ConfigurationManager.AppSettings["GeoID:RedirectUri"],
                Scope = "openid",
                ResponseType = "code id_token token",
                CookieManager = cookieManager,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = ConfigurationManager.AppSettings["GeoID:Issuer"]
                },
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = async context =>
                    {
                        var sessionState = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
                        context.AuthenticationTicket.Properties.ExpiresUtc = DateTimeOffset.UtcNow + sessionState.Timeout;
                        context.AuthenticationTicket.Properties.AllowRefresh = true;
                        context.AuthenticationTicket.Identity.AddClaim(new Claim("id_token", context.ProtocolMessage.IdToken));
                        context.AuthenticationTicket.Identity.AddClaim(new Claim("access_token", context.ProtocolMessage.AccessToken));

                        var authorizationService = context.OwinContext.GetAutofacLifetimeScope().Resolve<IGeonorgeAuthorizationService>();
                        var identity = context.AuthenticationTicket.Identity;
                        identity.AddClaims(await authorizationService.GetClaims(identity));
                    },
                    AuthenticationFailed = context =>
                    {
                        Log.Error("OpenID Connect authentication failed", context.Exception);
                        context.HandleResponse();
                        context.Response.Redirect(VirtualPathUtility.ToAbsolute("~/") + "?errormessage=" + Uri.EscapeDataString(context.Exception.Message));
                        return Task.FromResult(0);
                    },
                    RedirectToIdentityProvider = context =>
                    {
                        if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                        {
                            // AJAX requests can't follow a redirect to the identity provider, so the login would never
                            // complete and its nonce cookie would be left behind. Just return 401.
                            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            {
                                context.HandleResponse();
                                context.Response.StatusCode = 401;
                                return Task.FromResult(0);
                            }

                            // The new nonce cookie is already added to the response. Remove the ones from earlier
                            // logins that never reached the callback, so they don't pile up and overflow the request headers.
                            var nonceKeys = context.Request.Cookies
                                .Select(c => Uri.UnescapeDataString(c.Key))
                                .Where(k => k.StartsWith(NonceCookiePrefix))
                                .ToList();
                            foreach (var key in nonceKeys)
                                cookieManager.DeleteCookie(context.OwinContext, key, new CookieOptions { HttpOnly = true, Secure = context.Request.IsSecure, SameSite = Microsoft.Owin.SameSiteMode.None });
                        }
                        else if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Logout)
                        {
                            var idTokenClaim = context.OwinContext.Authentication.User.FindFirst("id_token");
                            if (idTokenClaim != null)
                                context.ProtocolMessage.IdTokenHint = idTokenClaim.Value;
                        }
                        return Task.FromResult(0);
                    }
                }
            });
        }
    }
}
