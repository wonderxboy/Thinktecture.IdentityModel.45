using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Thinktecture.IdentityModel.Web
{
    public class PassiveModuleConfiguration
    {
        public static void SuppressSecurityTokenExceptions(
                    string redirectPath = "~/",
                    Action<SecurityTokenException> logger = null)
        {
            HttpContext.Current.ApplicationInstance.Error +=
                delegate(object sender, EventArgs e)
                {
                    var ctx = HttpContext.Current;
                    var ex = ctx.Error;

                    SecurityTokenException ste = ex as SecurityTokenException;
                    if (ste != null)
                    {
                        var sam = FederatedAuthentication.SessionAuthenticationModule;
                        if (sam != null) sam.SignOut();

                        ctx.ClearError();

                        if (logger != null) logger(ste);

                        ctx.Response.Redirect(redirectPath);
                    }
                };
        }

        public static void CacheSessionsOnServer(bool checkForSessionSecurityTokenCache = true)
        {
            if (checkForSessionSecurityTokenCache && 
                !(FederatedAuthentication.FederationConfiguration.IdentityConfiguration.Caches.SessionSecurityTokenCache is PassiveRepositorySessionSecurityTokenCache))
            {
                throw new Exception("SessionSecurityTokenCache not configured.");
            }

            SessionAuthenticationModule sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null) throw new ArgumentException("SessionAuthenticationModule is null");

            sam.IsReferenceMode = true;
        }

        public static void EnableSlidingSessionExpirations()
        {
            SessionAuthenticationModule sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null) throw new ArgumentException("SessionAuthenticationModule is null");

            sam.SessionSecurityTokenReceived +=
                delegate(object sender, SessionSecurityTokenReceivedEventArgs e)
                {
                    var token = e.SessionToken;
                    var duration = token.ValidTo.Add(sam.FederationConfiguration.IdentityConfiguration.MaxClockSkew).Subtract(token.ValidFrom);
                    if (duration <= TimeSpan.Zero) return;

                    var diff = token.ValidTo.Add(sam.FederationConfiguration.IdentityConfiguration.MaxClockSkew).Subtract(DateTime.UtcNow);
                    if (diff <= TimeSpan.Zero) return;

                    var halfWay = duration.TotalMinutes / 2;
                    var timeLeft = diff.TotalMinutes;
                    if (timeLeft <= halfWay)
                    {
                        e.ReissueCookie = true;
                        e.SessionToken =
                            new SessionSecurityToken(token.ClaimsPrincipal, duration)
                            {
                                IsPersistent = token.IsPersistent,
                                IsReferenceMode = token.IsReferenceMode
                            };
                    }
                };
        }

        private static string WebApiControllerName = "System.Web.Http.WebHost.HttpControllerHandler";

        public static void SuppressLoginRedirectsForApiCalls()
        {
            var app = HttpContext.Current.ApplicationInstance;
            app.PostMapRequestHandler += 
                delegate
                {
                    var ctx = HttpContext.Current;
                    var req = new HttpRequestWrapper(ctx.Request);
                    var isApi = (req.IsAjaxRequest() ||
                                 ctx.Handler.GetType().FullName == WebApiControllerName);
                    ctx.Response.SuppressFormsAuthenticationRedirect = isApi;
                };

            var sam = FederatedAuthentication.WSFederationAuthenticationModule;
            if (sam != null)
            {
                sam.AuthorizationFailed +=
                    delegate(object sender, AuthorizationFailedEventArgs e)
                    {
                        var ctx = HttpContext.Current;
                        e.RedirectToIdentityProvider = !ctx.Response.SuppressFormsAuthenticationRedirect;
                    };
            }
        }
    }
}
