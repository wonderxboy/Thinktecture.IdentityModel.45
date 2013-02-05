using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityModel.Web
{
    public static class PassiveSessionConfiguration
    {
        public static void ConfigureDefaultSessionDuration(TimeSpan sessionDuration)
        {
            var handler = (SessionSecurityTokenHandler)FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers[typeof(SessionSecurityToken)];
            if (handler != null)
            {
                handler.TokenLifetime = sessionDuration;
            }
        }

        public static void EnablePersistentSessionCookies()
        {
            FederatedAuthentication.SessionAuthenticationModule.EnablePersistentSessionCookies();
            FederatedAuthentication.WSFederationAuthenticationModule.EnablePersistentSessionCookies();
        }
        static void EnablePersistentSessionCookies(this SessionAuthenticationModule sam)
        {
            if (sam == null) throw new ArgumentException("SessionAuthenticationModule is null");

            sam.SessionSecurityTokenCreated +=
                delegate(object sender, SessionSecurityTokenCreatedEventArgs e)
                {
                    e.SessionToken.IsPersistent = true;
                };
        }
        static void EnablePersistentSessionCookies(this WSFederationAuthenticationModule fam)
        {
            if (fam == null) return;

            fam.SessionSecurityTokenCreated +=
                delegate(object sender, SessionSecurityTokenCreatedEventArgs e)
                {
                    e.SessionToken.IsPersistent = true;
                };
        }

        public static void CacheSessionsOnServer()
        {
            FederatedAuthentication.SessionAuthenticationModule.CacheSessionsOnServer();
            FederatedAuthentication.WSFederationAuthenticationModule.CacheSessionsOnServer();
        }
        static void CacheSessionsOnServer(this SessionAuthenticationModule sam)
        {
            if (sam == null) throw new ArgumentException("SessionAuthenticationModule is null");

            sam.SessionSecurityTokenCreated +=
                delegate(object sender, SessionSecurityTokenCreatedEventArgs e)
                {
                    e.SessionToken.IsReferenceMode = true;
                };
        }
        static void CacheSessionsOnServer(this WSFederationAuthenticationModule fam)
        {
            if (fam == null) return;

            fam.SessionSecurityTokenCreated +=
                delegate(object sender, SessionSecurityTokenCreatedEventArgs e)
                {
                    e.SessionToken.IsReferenceMode = true;
                };
        }

        public static void EnableSlidingExpirations()
        {
            FederatedAuthentication.SessionAuthenticationModule.EnableSlidingExpirations();
        }
        static void EnableSlidingExpirations(this SessionAuthenticationModule sam)
        {
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
