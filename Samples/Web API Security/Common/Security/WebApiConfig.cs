using System.IdentityModel.Selectors;
using System.Web.Http;
using Thinktecture.IdentityModel.Tokens.Http;

namespace Thinktecture.Samples.Security
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // authentication configuration for identity controller
            var authentication = CreateAuthenticationConfiguration();

            #region session token support
            // session support - add a handler at the root
            var sessionTokenAuthentication = CreateSessionTokenAuthenticationConfiguration();
            config.MessageHandlers.Add(new AuthenticationHandler(sessionTokenAuthentication));
            
            // enable sessions on identity controller
            authentication.EnableSessionToken = true;
            
            // synchronize signing keys
            authentication.SessionToken.SigningKey = sessionTokenAuthentication.SessionToken.SigningKey;
            #endregion

            // route to identity controller
            config.Routes.MapHttpRoute(
                name: "Identity",
                routeTemplate: "api/identity",
                defaults: new { controller = "identity" },
                constraints: null,
                handler: new AuthenticationHandler(authentication, config)
            );

            // default API route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        private static AuthenticationConfiguration CreateSessionTokenAuthenticationConfiguration()
        {
            var config = new AuthenticationConfiguration
            {
                RequireSsl = false,
                EnableSessionToken = true,
                ClaimsAuthenticationManager = new ClaimsTransformer()
            };

            config.AddBasicAuthentication((u, p) => u == p);

            return config;
        }

        private static AuthenticationConfiguration CreateAuthenticationConfiguration()
        {
            var authentication = new AuthenticationConfiguration 
            {
                ClaimsAuthenticationManager = new ClaimsTransformer(),
                RequireSsl = false,
                EnableSessionToken = true
            };

            #region Basic Authentication
            authentication.AddBasicAuthentication((username, password) 
                => UserCredentials.Validate(username, password));
            #endregion

            #region IdentityServer JWT
            authentication.AddJsonWebToken(
                Constants.IdSrv.IssuerUri,
                Constants.Audience,
                Constants.IdSrv.SigningKey);
            #endregion

            #region Access Control Service JWT
            authentication.AddJsonWebToken(
                Constants.ACS.IssuerUri,
                Constants.Audience,
                Constants.ACS.SigningKey,
                AuthenticationOptions.ForAuthorizationHeader(Constants.ACS.Scheme));
            #endregion

            #region #IdentityServer SAML
            authentication.AddSaml2(
                issuerThumbprint: Constants.IdSrv.SigningCertThumbprint,
                issuerName: Constants.IdSrv.IssuerUri,
                audienceUri: Constants.Realm,
                certificateValidator: X509CertificateValidator.None,
                options: AuthenticationOptions.ForAuthorizationHeader(Constants.IdSrv.SamlScheme));
            #endregion

            return authentication;
        }
    }
}