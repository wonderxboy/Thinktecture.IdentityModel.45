using Newtonsoft.Json.Linq;
using System;
using System.IO;
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

            var client = new HttpClient { BaseAddress = new Uri("http://localhost:7526/api/UserData/") };
            //client.SetToken("FacebookToken", "CAAD4QWAiyZBoBAMhspvWnQl7gC33XtGmR7vljcyEKzjXMZAIZC7xv8FWHJS0vSPhVZC4TWd6BgV0OLLs4UXbf0nxmYfeH609Dc3AA1YDqNhj4cyqZChmZCtS3Gr7vUrbQLFm4kUVWOqSgUBMFdja5ZBeiGPfEyCmfZA558MGuYh4gdVRRbL532WodJkZCyhLqbHt3koHtgSUBsQZDZD");
            client.SetBasicAuthentication("seandong1", "12345678");

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
            var client = new HttpClient { BaseAddress = new Uri("http://localhost:7526/api/") };

            client.SetToken("Session", token);

            // some alternative header for session token 
            //client.DefaultRequestHeaders.Add("X-Session", "Session " + token);

            while (true)
            {
                "Calling service.".ConsoleYellow();

                Helper.Timer(() =>
                {
                    var response = client.GetAsync("userdata").Result;
                    response.EnsureSuccessStatusCode();

                    //var claims = response.Content.ReadAsAsync<ViewClaims>().Result;
                    //Helper.ShowConsole(claims);
                    var re = response.Content.ReadAsStringAsync().Result;
                    re.ConsoleGreen();
                });

                Console.ReadLine();
            }
        }
    }
}
