using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Owin;
using Thinktecture.IdentityModel.Http.Hawk.Owin.Extensions;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Http.Hawk.Core.MessageContracts;

namespace Hawk.Samples.WebApi.Owin
{
    class Program
    {
        static void Main(string[] args)
        {
            const string baseUrl = "http://localhost:12345/";

            using (WebApp.Start<Startup>(new StartOptions(baseUrl)))
            {
                Console.WriteLine("Press Enter to terminate.");
                Console.Read();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var credentialStorage = new List<Credential>()
            {
                new Credential()
                {
                    Id = "dh37fgj492je",
                    Algorithm = SupportedAlgorithms.SHA256,
                    User = "Steve",
                    Key = "werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn"
                }
            };

            var options = new Options()
            {
                ClockSkewSeconds = 60,
                LocalTimeOffsetMillis = 0,
                CredentialsCallback = (id) => credentialStorage.FirstOrDefault(c => c.Id == id),
                ResponsePayloadHashabilityCallback = (r) => true,
                VerificationCallback = (request, ext) =>
                {
                    if (String.IsNullOrEmpty(ext))
                        return true;

                    string name = "X-Request-Header-To-Protect";
                    return ext.Equals(name + ":" + request.Headers[name].First());
                }
            };

            app.UseHawkAuthentication(new HawkAuthenticationOptions(options));

            var config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                "DefaultWebApi",
                "{controller}/{id}",
                new { id = RouteParameter.Optional });

            app.UseWebApi(config);


        }
    }

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
