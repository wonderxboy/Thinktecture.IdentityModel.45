using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Thinktecture.Samples.Data
{
    public class ViewClaim
    {
        public static IEnumerable<ViewClaim> GetAll()
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

        public string Type { get; set; }
        public string Value { get; set; }
    }
}