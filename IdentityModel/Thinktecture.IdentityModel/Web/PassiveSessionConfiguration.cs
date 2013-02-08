using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Services.Tokens;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityModel.Web
{
    public static class PassiveSessionConfiguration
    {
        public static void ConfigureSessionCache(ITokenCacheRepository tokenCacheRepository)
        {
            if (!(FederatedAuthentication.FederationConfiguration.IdentityConfiguration.Caches.SessionSecurityTokenCache is PassiveRepositorySessionSecurityTokenCache))
            {
                FederatedAuthentication.FederationConfiguration.IdentityConfiguration.Caches.SessionSecurityTokenCache = new PassiveRepositorySessionSecurityTokenCache(tokenCacheRepository);
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
        
        public static void ConfigureMackineKeyForSessionTokens()
        {
            var handler = (SessionSecurityTokenHandler)FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers[typeof(SessionSecurityToken)];
            if (!(handler is MachineKeySessionSecurityTokenHandler))
            {
                var mkssth = new MachineKeySessionSecurityTokenHandler();
                if (handler != null) mkssth.TokenLifetime = handler.TokenLifetime;
                FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers.AddOrReplace(mkssth);
            }
        }

        public static void ConfigurePersistentSessions(TimeSpan persistentDuration)
        {
            FederatedAuthentication.FederationConfiguration.WsFederationConfiguration.PersistentCookiesOnPassiveRedirects = true;
            FederatedAuthentication.FederationConfiguration.CookieHandler.PersistentSessionLifetime = persistentDuration;
        }
    }
}
