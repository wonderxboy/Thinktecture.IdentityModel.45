using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.Samples;

namespace SimpleHttpClient
{
    class Program
    {
        static Uri _baseAddress = new Uri(Constants.WebHostBaseAddress);
        static string tokenType = "swt";

        static void Main(string[] args)
        {
            var token = GetToken();

            Console.ReadLine();
        }

        private static string GetToken()
        {
            "Requesting token".ConsoleYellow();

            var client = new HttpClient
            {
                BaseAddress = new Uri(Constants.IdSrv.SimpleHttpEndpoint)
            };

            client.SetBasicAuthentication("bob", "abc!123");

            var response = client.GetAsync("?realm=" + Constants.Realm + "&tokentype=" + tokenType).Result;
            response.EnsureSuccessStatusCode();

            var tokenResponse = response.Content.ReadAsStringAsync().Result;
            var token = JObject.Parse(tokenResponse)["access_token"].ToString();

            Console.WriteLine(token);

            return token;
        }
    }
}
