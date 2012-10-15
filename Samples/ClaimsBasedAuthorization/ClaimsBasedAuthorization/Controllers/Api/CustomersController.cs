using System.Web.Http;
using Thinktecture.IdentityModel.Authorization;

namespace ClaimsBasedAuthorization.Controllers.Api
{
    public class CustomersController : ApiController
    {
        public string Get()
        {
            return "OK";
        }

        public string Get(int id)
        {
            var result = ClaimsAuthorization.CheckAccess("Get", "Customer", id.ToString());

            return "OK " + id.ToString();
        }
    }
}
