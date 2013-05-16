using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.Samples;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityModel;

namespace BasicAuthAndClientCert
{
    class Program
    {
        static Uri _baseAddress = new Uri(Constants.WebHostBaseAddress);
        //static Uri _baseAddress = new Uri(Constants.SelfHostBaseAddress);

        static void Main(string[] args)
        {
            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(
                X509.CurrentUser.My.SubjectDistinguishedName.Find("CN=Client").First());

            var client = new HttpClient(handler) {
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
