using System.Collections.Generic;
using System.Web.Http;
using Thinktecture.Samples.Data;
using Thinktecture.Samples.Security;

namespace Thinktecture.Samples.Controller
{
    [Authorize]
    public class IdentityController : ApiController
    {
        public IEnumerable<ViewClaim> Get()
        {
            return Claims.Get();
        }
    }
}
