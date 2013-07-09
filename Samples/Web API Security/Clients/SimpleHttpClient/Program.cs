using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityModel.Tokens;
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
            
            // swt parsing and validationn
            ValidateSwtToken(token);

            Console.ReadLine();
        }

        private static void ValidateSwtToken(string tokenString)
        {
            var configuration = new SecurityTokenHandlerConfiguration();
            var validationKey = new InMemorySymmetricSecurityKey(Convert.FromBase64String(Constants.IdSrv.SigningKey));

            // audience validation
            configuration.AudienceRestriction.AllowedAudienceUris.Add(new Uri(Constants.Realm));

            // signature & issuer validation
            var resolverTable = new Dictionary<string, IList<SecurityKey>>
            {
                { Constants.IdSrv.IssuerUri, new SecurityKey[] { validationKey } }
            };

            configuration.IssuerTokenResolver = new NamedKeyIssuerTokenResolver(resolverTable);

            var handler = new SimpleWebTokenHandler();
            handler.Configuration = configuration;

            var token = handler.ReadToken(tokenString);
            var ids = handler.ValidateToken(token);

            "\n\nValidated Claims:".ConsoleYellow();
            foreach (var claim in ids.First().Claims)
            {
                Console.WriteLine("{0}\n {1}\n", claim.Type, claim.Value);
            }
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
