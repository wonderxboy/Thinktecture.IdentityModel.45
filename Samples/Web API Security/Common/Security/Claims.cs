using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Thinktecture.Samples.Data;

namespace Thinktecture.Samples.Security
{
    public static class Claims
    {
        public static IEnumerable<ViewClaim> Get()
        {
            var principal = ClaimsPrincipal.Current;

            var claims = new List<ViewClaim>(
                from c in principal.Claims
                select new ViewClaim
                {
                    Type = c.Type,
                    Value = c.Value
                });

            return claims;
        }
    }
}
