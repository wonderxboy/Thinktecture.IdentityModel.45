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
    }
}
