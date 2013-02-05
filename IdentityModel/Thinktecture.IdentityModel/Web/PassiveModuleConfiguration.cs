using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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

        public static void CacheSessionsOnServer()
        {
            SessionAuthenticationModule sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null) throw new ArgumentException("SessionAuthenticationModule is null");
            sam.IsReferenceMode = true;
        }

        public static void EnableSlidingExpirations()
        {
            SessionAuthenticationModule sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null) throw new ArgumentException("SessionAuthenticationModule is null");

            sam.SessionSecurityTokenReceived +=
                delegate(object sender, SessionSecurityTokenReceivedEventArgs e)
                {
                    var token = e.SessionToken;
                    var duration = token.ValidTo.Subtract(token.ValidFrom);
                    var halfWay = duration.TotalMinutes / 2;

                    var diff = token.ValidTo.Subtract(DateTime.UtcNow);
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
    }
}
