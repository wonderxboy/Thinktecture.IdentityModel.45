using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Principal;
using Thinktecture.IdentityModel;

namespace WebApiInAppWindows.Security
{
    public class ClaimsTransformer : ClaimsAuthenticationManager
    {
        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            if (!incomingPrincipal.Identity.IsAuthenticated)
            {
                return base.Authenticate(resourceName, incomingPrincipal);
            }

            var name = GetName(incomingPrincipal);
            var role = GetRole(incomingPrincipal);

            var principal = Principal.Create("Windows", name, role);

            EstablishSession(principal);
            
            return principal;
        }

        private void EstablishSession(ClaimsPrincipal principal)
        {
            if (FederatedAuthentication.SessionAuthenticationModule != null)
            {
                var sessionToken = new SessionSecurityToken(principal);
                FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionToken);
            }
        }

        private Claim GetRole(ClaimsPrincipal incomingPrincipal)
        {
            var sid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

            if (incomingPrincipal.IsInRole(sid.Value))
            {
                return new Claim(ClaimTypes.Role, "Operator");
            }
            else
            {
                return new Claim(ClaimTypes.Role, "User");
            }
        }

        private Claim GetName(ClaimsPrincipal incomingPrincipal)
        {
            return incomingPrincipal.FindFirst(ClaimTypes.Name);
        }
    }
}