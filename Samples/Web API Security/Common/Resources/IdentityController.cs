using System.Web.Http;

namespace Thinktecture.Samples
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
