using System;
using System.Net.Http;
using Thinktecture.IdentityModel.Clients;
using Thinktecture.IdentityModel.Extensions;

namespace Thinktecture.Samples
{
    class Program
    {
        static Uri _baseAddress = new Uri(Constants.WebHostBaseAddress);
        //static Uri _baseAddress = new Uri(Constants.SelfHostBaseAddress);

        static void Main(string[] args)
        {
            var token = RequestToken();
            CallService(token);
        }

        private static string RequestToken()
        {
            "Requesting token.".ConsoleYellow();

            var client = new OAuth2Client(
                new Uri(Constants.IdSrv.OAuth2TokenEndpoint),
                Constants.IdSrv.OAuthClientName,
                Constants.IdSrv.OAuthClientSecret);

            var response = client.RequestAccessTokenUserName("bob", "abc!123", Constants.Scope);
            return response.AccessToken;
        }

        private static void CallService(string token)
        {
            var client = new HttpClient {
                BaseAddress = _baseAddress
            };

            client.SetBearerToken(token);

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
