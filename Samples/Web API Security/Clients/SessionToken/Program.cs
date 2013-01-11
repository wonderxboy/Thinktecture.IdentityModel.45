using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
            while (true)
            {
                RequestSessionToken();
                Console.ReadLine();
            }
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
            
            "\nSession Token:".ConsoleRed();
            Console.WriteLine(json.ToString());

            return token;
        }
    }
}
