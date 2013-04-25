using Microsoft.IdentityModel.Tokens.JWT;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.ServiceModel.Security.Tokens;
using Thinktecture.IdentityModel.Constants;
using Thinktecture.IdentityModel.Tokens.Http;

namespace Thinktecture.Samples.Security
{
    public static class AuthenticationConfigurationExtensionsForMsftJwt
    {
        public static void AddMsftJsonWebToken(this AuthenticationConfiguration configuration, string issuer, string audience, string signingKey)
        {
            var validationParameters = new TokenValidationParameters()
            {
                AllowedAudience = audience,
                SigningToken = new BinarySecretSecurityToken(Convert.FromBase64String(signingKey)),
                ValidIssuer = issuer,
                ValidateExpiration = true
            };

            var handler = new JWTSecurityTokenHandlerWrapper(validationParameters);

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = AuthenticationOptions.ForAuthorizationHeader(JwtConstants.Bearer),
                Scheme = AuthenticationScheme.SchemeOnly(JwtConstants.Bearer)
            });


            ////var handler = new JWTSecurityTokenHandler();

            //var handlerConfiguration = new SecurityTokenHandlerConfiguration();
            //handlerConfiguration.AudienceRestriction.AllowedAudienceUris.Add(new Uri(audience));


            //var registry = new ValidatingIssuerNameRegistry(new IssuingAuthority(issuer));

            //handlerConfiguration.IssuerNameRegistry = registry;

            //var issuerResolver = new NamedKeyIssuerTokenResolver(
            //    new Dictionary<string, List<SecurityKey>> 
            //    {
            //        { 
            //            issuer, new List<SecurityKey> 
            //            {
            //                new InMemorySymmetricSecurityKey(Convert.FromBase64String(signingKey))
            //            }
            //        }
            //    });
            //handlerConfiguration.IssuerTokenResolver = issuerResolver;

            //var handler = new JWTSecurityTokenHandler();
            //handler.Configuration = handlerConfiguration;

           

            //TokenValidationParameters validationParameters = new TokenValidationParameters
            //{
            //    ClockSkewInSeconds = Convert.ToUInt32(base.Configuration.MaxClockSkew.TotalSeconds)
            //};
            //List<string> list = new List<string>();
            //foreach (Uri uri in base.Configuration.AudienceRestriction.AllowedAudienceUris)
            //{
            //    list.Add(uri.OriginalString);
            //}
            //validationParameters.AllowedAudiences = list;
            //validationParameters.SigningTokenResolver = base.Configuration.IssuerTokenResolver;
            //validationParameters.X509CertificateValidator = base.Configuration.CertificateValidator;
            //validationParameters.SaveBootstrapContext = base.Configuration.SaveBootstrapContext;
            //validationParameters.IssuerNameRegistry = base.Configuration.IssuerNameRegistry;
            //List<ClaimsIdentity> list2 = new List<ClaimsIdentity>(this.ValidateToken(jwt, validationParameters).Identities);
            //return list2.AsReadOnly();



            //var config = new SecurityTokenHandlerConfiguration();
            

            //var handler = new JsonWebTokenHandler();
            //handler.Configuration = config;

           
        }
    }

    class JWTSecurityTokenHandlerWrapper : JWTSecurityTokenHandler
    {
        TokenValidationParameters validationParams;
        public JWTSecurityTokenHandlerWrapper(TokenValidationParameters validationParams)
        {
            this.validationParams = validationParams;
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<System.Security.Claims.ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            var jwt = token as JWTSecurityToken;
            var list = new List<ClaimsIdentity>(this.ValidateToken(jwt, validationParams).Identities);
            return list.AsReadOnly();
        }
    }
}
