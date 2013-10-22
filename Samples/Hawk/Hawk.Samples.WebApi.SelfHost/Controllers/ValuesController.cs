using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Hawk.Samples.WebApi.SelfHost.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse<string>(HttpStatusCode.OK, "Hello, " + User.Identity.Name);
        }

        public HttpResponseMessage Post([FromBody]string name)
        {
            string message = String.Format("Hello, {0}. Thanks for flying Hawk", name);
            return Request.CreateResponse<string>(HttpStatusCode.OK, message);
        }
    }
}
