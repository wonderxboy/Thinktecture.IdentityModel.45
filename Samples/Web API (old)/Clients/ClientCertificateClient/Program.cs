using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.Samples;
using Thinktecture.IdentityModel.Extensions;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityModel;

namespace ClientCertificateClient
{
    class Program
    {
        static Uri _baseAddress = new Uri(Constants.WebHostBaseAddress);
        //static Uri _baseAddress = new Uri(Constants.SelfHostBaseAddress);

        static void Main(string[] args)
        {
            while (true)
            {
                Helper.Timer(() =>
                {
                    "Calling Service\n".ConsoleYellow();

                    var handler = new WebRequestHandler();
                    handler.ClientCertificates.Add(
                        X509.
                        CurrentUser.
                        My.
                        SubjectDistinguishedName.
                        Find("CN=Client").First());

                    var client = new HttpClient(handler) { BaseAddress = _baseAddress };
                    
                    var response = client.GetAsync("identity").Result;
                    response.EnsureSuccessStatusCode();

                    var identity = response.Content.ReadAsAsync<Identity>().Result;
                    identity.ShowConsole();
                });

                Console.ReadLine();
            }
        }
    }
}
