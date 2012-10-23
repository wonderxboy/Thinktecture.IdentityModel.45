using System.Web.Http;

namespace FormsAndBasicAuth.Controllers.Api
{
    public class GreetingController : ApiController
    {
        [Authorize]
        public string Get()
        {
            return User.Identity.Name;
        }
    }
}
