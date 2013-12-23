using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Thinktecture.IdentityModel.Http.Hawk.Client;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Http.Hawk.WebApi;

namespace Hawk.Samples.Client.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string uri = "http://localhost:12345/values";

            var credential = new Credential()
            {
                Id = "dh37fgj492je",
                Algorithm = SupportedAlgorithms.SHA256,
                User = "Steve",
                Key = "werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn"
            };

            // GET and POST using the Authorization header
            var options = new ClientOptions()
            {
                CredentialsCallback = () => credential,
                RequestPayloadHashabilityCallback = (r) => true,
                NormalizationCallback = (req) =>
                {
                    string name = "X-Request-Header-To-Protect";
                    return req.Headers.ContainsKey(name) ? 
                                name + ":" + req.Headers[name].First() : null;
                }
            };

            var handler = new HawkValidationHandler(options);

            HttpClient client = HttpClientFactory.Create(handler);
            client.DefaultRequestHeaders.Add("X-Request-Header-To-Protect", "secret");

            var response = client.GetAsync(uri).Result;
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            response = client.PostAsJsonAsync(uri, credential.User).Result;
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            // GET using Bewit
            var hawkClient = new HawkClient(options);
            var request = new HttpRequestMessage() { RequestUri = new Uri(uri) };

            string bewit = hawkClient.CreateBewit(new WebApiRequestMessage(request),
                                                        lifeSeconds: 60);

            // Bewit is handed off to a client needing temporary access to the resource.
            var clientNeedingTempAccess = new WebClient();
            var resource = clientNeedingTempAccess.DownloadString(uri + "?bewit=" + bewit);
            Console.WriteLine(resource);

            Console.Read();
        }
    }
}
