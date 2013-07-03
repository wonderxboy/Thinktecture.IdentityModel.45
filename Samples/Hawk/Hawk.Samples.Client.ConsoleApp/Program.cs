using System;
using System.Net;
using System.Net.Http;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Client;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;

namespace Hawk.Samples.Client.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var credential = new Credential()
            {
                Id = "dh37fgj492je",
                Algorithm = SupportedAlgorithms.SHA256,
                User = "Steve",
                Key = "werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn"
            };

            string uri = "http://localhost:12345/api/values";

            var client = new HttpClient();

            // GET using the Authorization header
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("X-Request-Header-To-Protect", "Swoosh");

            var hawkClient = new HawkClient(() => credential);
            hawkClient.ApplicationSpecificData = "X-Request-Header-To-Protect:Swoosh"; // Normalized header
            hawkClient.CreateClientAuthorizationAsync(request).Wait();

            var response = client.SendAsync(request).Result;
            var isAuthentic = hawkClient.AuthenticateAsync(response).Result;
            Console.WriteLine(isAuthentic ? response.Content.ReadAsStringAsync().Result : "Response is Tampered");

            // GET using Bewit
            hawkClient = new HawkClient(() => credential);
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
