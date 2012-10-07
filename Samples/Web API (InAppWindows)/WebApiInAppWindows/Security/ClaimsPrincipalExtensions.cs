using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace WebApiInAppWindows.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsOperator(this ClaimsPrincipal principal)
        {
            return principal.HasClaim(ClaimTypes.Role, "Operator");
        }
    }
}