using System.Collections.Generic;
using System.Web.Http;
using Thinktecture.Samples.Data;

namespace Thinktecture.Samples.Resources
{
    [Authorize]
    public class IdentityController : ApiController
    {
        public IEnumerable<ViewClaim> Get()
        {
            return ViewClaim.GetAll();
        }
    }
}
