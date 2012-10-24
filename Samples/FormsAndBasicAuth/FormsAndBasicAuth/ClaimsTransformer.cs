using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Thinktecture.IdentityModel;

namespace FormsAndBasicAuth
{
    public class ClaimsTransformer : ClaimsAuthenticationManager
    {
        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            if (!incomingPrincipal.Identity.IsAuthenticated)
            {
                return base.Authenticate(resourceName, incomingPrincipal);
            }

            var name = incomingPrincipal.Identity.Name;

            return Principal.Create(
                "Custom", 
                new Claim(ClaimTypes.Name, name + " (transformed)"));
        }
    }
}