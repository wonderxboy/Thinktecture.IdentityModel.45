using System;
using System.Net.Http;
using Thinktecture.IdentityModel.Extensions;

namespace Thinktecture.Samples
{
    class Program
    {
        static Uri _baseAddress = new Uri(Constants.WebHostBaseAddress);
        //static Uri _baseAddress = new Uri(Constants.SelfHostBaseAddress);

        static void Main(string[] args)
        {
            var client = new HttpClient {
                BaseAddress = _baseAddress
            };

            client.SetBasicAuthentication("bob", "bob");

            while (true)
            {
                Helper.Timer(() =>
                {
                    "Calling service.".ConsoleYellow();

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
