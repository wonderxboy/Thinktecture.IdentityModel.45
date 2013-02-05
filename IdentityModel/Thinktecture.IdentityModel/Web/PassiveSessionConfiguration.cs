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
        public static void ConfigureSessionCache(ITokenCacheRepositoryFactory factory)
        {
            if (!(FederatedAuthentication.FederationConfiguration.IdentityConfiguration.Caches.SessionSecurityTokenCache is PassiveRepositorySessionSecurityTokenCache))
            {
                FederatedAuthentication.FederationConfiguration.IdentityConfiguration.Caches.SessionSecurityTokenCache = new PassiveRepositorySessionSecurityTokenCache(factory);
            }
        }

        public static void ConfigureDefaultSessionDuration(TimeSpan sessionDuration)
        {
            var handler = (SessionSecurityTokenHandler)FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers[typeof(SessionSecurityToken)];
            if (handler != null)
            {
                handler.TokenLifetime = sessionDuration;
            }
        }

        public static void ConfigurePersistentSessions(TimeSpan persistentDuration)
        {
            FederatedAuthentication.FederationConfiguration.WsFederationConfiguration.PersistentCookiesOnPassiveRedirects = true;
            FederatedAuthentication.FederationConfiguration.CookieHandler.PersistentSessionLifetime = persistentDuration;
        }
    }
}
