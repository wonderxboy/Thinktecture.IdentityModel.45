using System.Web.Http;
using Thinktecture.Samples.Data;

namespace Thinktecture.Samples.Resources
{
    [Authorize]
    public class IdentityController : ApiController
    {
        public ViewClaims Get()
        {
            return ViewClaims.GetAll();
        }
    }
}
