using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace WebApiInAppWindows.Controllers.Api
{
    [Authorize]
    public class IdentityController : ApiController
    {
        public IEnumerable<ViewClaim> Get()
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

    public class ViewClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
