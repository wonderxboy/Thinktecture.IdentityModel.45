using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.Samples;

namespace SessionToken
{
    class Program
    {
        static Uri _baseAddress = new Uri(Constants.WebHostBaseAddress);
        //static Uri _baseAddress = new Uri(Constants.SelfHostBaseAddress);

        static void Main(string[] args)
        {
            var sessionToken = RequestSessionToken();
            CallService(sessionToken);
        }

        private static string RequestSessionToken()
        {
            "Requesting session token\n".ConsoleYellow();

            var client = new HttpClient { BaseAddress = _baseAddress };
            client.SetBasicAuthentication("bob", "bob");

            var response = client.GetAsync("token").Result;
            response.EnsureSuccessStatusCode();

            var tokenResponse = response.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(tokenResponse);
            var token = json["access_token"].ToString();
            var expiresIn = int.Parse(json["expires_in"].ToString());
            var expiration = DateTime.UtcNow.AddSeconds(expiresIn);

            "\nSession Token:".ConsoleRed();
            Console.WriteLine(json.ToString());

            "\nExpiration:".ConsoleRed();
            Console.WriteLine(expiration.ToLongDateString() + " " + expiration.ToLongTimeString());

            return token;
        }

        private static void CallService(string token)
        {
            var client = new HttpClient { BaseAddress = _baseAddress };

            client.SetToken("Session", token);

            while (true)
            {
                "Calling service.".ConsoleYellow();

                Helper.Timer(() =>
                {
                    var response = client.GetAsync("identity").Result;
                    response.EnsureSuccessStatusCode();

                    var claims = response.Content.ReadAsAsync<ViewClaims>().Result;
                    Helper.ShowConsole(claims);
                });

                Console.ReadLine();
            }
        }
    }
}
