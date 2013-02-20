using System.IdentityModel.Selectors;
using System.Web.Http;
using Thinktecture.IdentityModel.Http.Cors;
using Thinktecture.IdentityModel.Tokens.Http;

namespace Thinktecture.Samples.Security
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            CorsConfiguration corsConfig = new CorsConfiguration();
            corsConfig.AllowAll();
            var corsHandler = new Thinktecture.IdentityModel.Http.Cors.WebApi.CorsMessageHandler(corsConfig, config);
            config.MessageHandlers.Add(corsHandler);

            // authentication configuration for identity controller
            var authentication = CreateAuthenticationConfiguration();
            config.MessageHandlers.Add(new AuthenticationHandler(authentication));

            // default API route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
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