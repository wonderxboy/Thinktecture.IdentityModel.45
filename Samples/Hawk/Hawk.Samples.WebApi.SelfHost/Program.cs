using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Http.Hawk.WebApi;

namespace Hawk.Samples.WebApi.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new HttpSelfHostConfiguration("http://localhost:12345");

            configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

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

            Func<string, Credential> credentialsCallback = (id) => credentialStorage.FirstOrDefault(c => c.Id == id);

            // Client and server decided that application specific data (ext) will be in the form of
            // header name and header value separated by a colon, like so: X-Header-To-Protect:Swoosh.

            Func<HttpResponseMessage, string> normalizationCallback = (response) =>
            {
                string header = "X-Response-Header-To-Protect";

                return response.Headers.Contains(header) ?
                            String.Format("{0}:{1}", header, response.Headers.GetValues(header).FirstOrDefault()) :
                                null;
            };

            Func<HttpRequestMessage, string, bool> verificationCallback = (request, appSpecificData) =>
            {
                if (String.IsNullOrEmpty(appSpecificData))
                    return true; // Nothing to check against

                var parts = appSpecificData.Split(':');
                string headerName = parts[0];
                string value = parts[1];

                if (request.Headers.Contains(headerName) &&
                    request.Headers.GetValues(headerName).First().Equals(value))
                    return true;

                return false;
            };

            configuration.MessageHandlers.Add(
                new HawkAuthenticationHandler(credentialsCallback, normalizationCallback, verificationCallback));

            using (HttpSelfHostServer server = new HttpSelfHostServer(configuration))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to terminate the server...");
                Console.ReadLine();
            }
        }
    }
}
