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
            var response = Request.CreateResponse<string>(HttpStatusCode.OK, "Thanks for flying Hawk");
            response.Headers.Add("X-Response-Header-To-Protect", "Swoop"); // Sensitive header to be protected

            return response;
        }

        public HttpResponseMessage Post([FromBody]string name)
        {
            string message = String.Format("Hello, {0}. Thanks for flying Hawk", name);

            var response = Request.CreateResponse<string>(HttpStatusCode.OK, message);
            response.Headers.Add("X-Response-Header-To-Protect", "Swoop"); // Sensitive header to be protected

            return response;
        }
    }
}
