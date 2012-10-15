using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Thinktecture.IdentityModel.Authorization;
using Thinktecture.IdentityModel.Authorization.WebApi;

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
            var result = ClaimsAuthorization.CheckAccess("GetCustomer", id.ToString());

            return "OK " + id.ToString();
        }
    }
}
