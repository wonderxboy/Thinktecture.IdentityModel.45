using System;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Security;
using Thinktecture.IdentityModel.Clients;
using Thinktecture.IdentityModel.Constants;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityModel.WSTrust;
using Thinktecture.Samples;

namespace SamlJwtConversionUsingAcs
{
    class Program
    {
        static Uri _baseAddress = new Uri(Constants.WebHostBaseAddress);
        //static Uri _baseAddress = new Uri(Constants.SelfHostBaseAddress);

        static void Main(string[] args)
        {
            var saml = GetSamlToken();
            var jwt = ConvertToJwt(saml);
            
            CallService(jwt);
        }

        private static string GetSamlToken()
        {
            "Requesting identity token".ConsoleYellow();

            var factory = new WSTrustChannelFactory(
                new UserNameWSTrustBinding(SecurityMode.TransportWithMessageCredential),
                Constants.IdSrv.WSTrustEndpoint);
            factory.TrustVersion = TrustVersion.WSTrust13;

            factory.Credentials.UserName.UserName = "bob";
            factory.Credentials.UserName.Password = "abc!123";

            var rst = new RequestSecurityToken
            {
                RequestType = RequestTypes.Issue,
                KeyType = KeyTypes.Bearer,
                TokenType = TokenTypes.Saml2TokenProfile11,
                AppliesTo = new EndpointReference(Constants.ACS.IssuerUri)
            };

            var token = factory.CreateChannel().Issue(rst) as GenericXmlSecurityToken;
            return token.TokenXml.OuterXml;
        }

        private static string ConvertToJwt(string samlToken)
        {
            "Converting token from SAML to JWT using ACS".ConsoleYellow();

            var client = new OAuth2Client(new Uri(Constants.ACS.OAuth2Endpoint));
            
            return client.RequestAccessTokenAssertion(
                samlToken,
                TokenTypes.Saml2TokenProfile11,
                Constants.Realm).AccessToken;
        }

        private static void CallService(string token)
        {
            var client = new HttpClient
            {
                BaseAddress = _baseAddress
            };

            client.SetToken(Constants.ACS.Scheme, token);

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
