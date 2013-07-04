using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Thinktecture.IdentityModel.Http.Hawk.Client;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Client;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;

namespace Hawk.Samples.Client.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string uri = "http://localhost:12345/api/values";
            string headerName = "X-Response-Header-To-Protect";

            var credential = new Credential()
            {
                Id = "dh37fgj492je",
                Algorithm = SupportedAlgorithms.SHA256,
                User = "Steve",
                Key = "werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn"
            };
            
            // GET and POST using the Authorization header
            var handler = new HawkValidationHandler(credentialsCallback: () => credential,
                                                        verificationCallback: (r, ext) =>
                                                            ext.Equals(headerName + ":" + r.Headers.GetValues(headerName).First()));
            HttpClient client = HttpClientFactory.Create(handler);
            
            var response = client.GetAsync(uri).Result;
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            response = client.PostAsJsonAsync(uri, credential.User).Result;
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            // GET using Bewit
            var hawkClient = new HawkClient(() => credential);
            string bewit = hawkClient.CreateBewitAsync(new HttpRequestMessage() { RequestUri = new Uri(uri) },
                                                        lifeSeconds:60).Result;

            // Bewit is handed off to a client needing temporary access to the resource.
            var clientNeedingTempAccess = new WebClient();
            var resource = clientNeedingTempAccess.DownloadString(uri + "?bewit=" + bewit);
            Console.WriteLine(resource);

            Console.Read();
        }
    }
}
