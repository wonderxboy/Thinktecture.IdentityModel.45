using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Web;

namespace WebApiInAppWindows.Security
{
    public class CustomClaimsPrincipal : ClaimsPrincipal
    {
        public CustomClaimsPrincipal(ClaimsPrincipal principal)
            : base(principal)
        { }

        public bool IsOperator
        {
            get
            {
                return HasClaim(ClaimTypes.Role, "Operator");
            }
        }
    }
}