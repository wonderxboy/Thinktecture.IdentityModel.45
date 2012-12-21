using System;
using System.Collections.Generic;
using System.Net.Http;
using Thinktecture.Samples;
using Thinktecture.Samples.Data;

namespace BasicAuthenticationClient
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
                    var response = client.GetAsync("identity").Result;
                    response.EnsureSuccessStatusCode();

                    var claims = response.Content.ReadAsAsync<IEnumerable<ViewClaim>>().Result;
                    Helper.ShowConsole(claims);
                });

                Console.ReadLine();
            }            
        }
    }
}
