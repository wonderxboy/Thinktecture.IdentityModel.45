using System.IdentityModel.Selectors;
using System.Web.Http;
using Thinktecture.IdentityModel.Http.Cors;
using Thinktecture.IdentityModel.Http.Cors.WebApi;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens.Http;
using Thinktecture.Samples.Security;

namespace Thinktecture.Samples.Security
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            CorsConfiguration corsConfig = new CorsConfiguration();
            corsConfig.AllowAll();
            var corsHandler = new CorsMessageHandler(corsConfig, config);
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

            //config.EnableSystemDiagnosticsTracing();
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
            authentication.AddBasicAuthentication(UserCredentials.Validate);
            #endregion

            #region IdentityServer JWT
            authentication.AddJsonWebToken(
                issuer: Constants.IdSrv.IssuerUri,
                audience: Constants.Audience,
                signingKey: Constants.IdSrv.SigningKey);
            #endregion

            #region Access Control Service JWT
            authentication.AddJsonWebToken(
                issuer: Constants.ACS.IssuerUri,
                audience: Constants.Audience,
                signingKey: Constants.ACS.SigningKey,
                scheme: Constants.ACS.Scheme);
            #endregion

            #region IdentityServer SAML
            authentication.AddSaml2(
                issuerThumbprint: Constants.IdSrv.SigningCertThumbprint,
                issuerName: Constants.IdSrv.IssuerUri,
                audienceUri: Constants.Realm,
                certificateValidator: X509CertificateValidator.None,
                options: AuthenticationOptions.ForAuthorizationHeader(Constants.IdSrv.SamlScheme),
                scheme: AuthenticationScheme.SchemeOnly(Constants.IdSrv.SamlScheme));
            #endregion

            #region Client Certificates
            authentication.AddClientCertificate(ClientCertificateMode.ChainValidation);
            #endregion

            return authentication;
        }
    }
}