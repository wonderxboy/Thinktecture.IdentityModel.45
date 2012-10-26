using System.Web.Http;
using Thinktecture.IdentityModel.Authorization;
using Thinktecture.IdentityModel.Authorization.WebApi;

namespace ClaimsBasedAuthorization.Controllers.Api
{
    public class CustomersController : ApiController
    {
        [ClaimsAuthorize("Read", "SomeData")]
        public string Get()
        {
            return "OK";
        }

        public string Get(int id)
        {
            var isAllowed = ClaimsAuthorization.CheckAccess("Get", "CustomerId", id.ToString());

            return "OK " + id.ToString();
        }
    }
}
