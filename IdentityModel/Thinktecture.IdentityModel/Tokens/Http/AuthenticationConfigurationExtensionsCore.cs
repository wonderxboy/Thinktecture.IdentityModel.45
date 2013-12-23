/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security.Tokens;
using Thinktecture.IdentityModel.Constants;

namespace Thinktecture.IdentityModel.Tokens.Http
{
    public static class AuthenticationConfigurationExtensionsCore
    {
        public static void AddAccessKey(this AuthenticationConfiguration configuration, SimpleSecurityTokenHandler handler, AuthenticationOptions options)
        {
            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = options,
            });
        }

        public static void AddAccessKey(this AuthenticationConfiguration configuration, SimpleSecurityTokenHandler.ValidateTokenDelegate validateTokenDelegate, AuthenticationOptions options)
        {
            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { new SimpleSecurityTokenHandler(validateTokenDelegate) },
                Options = options,
            });
        }

        public static void AddJsonWebToken(
            this AuthenticationConfiguration configuration, 
            string issuer, 
            string audience, 
            string signingKey, 
            Dictionary<string, string> claimMappings = null)
        {
            var validationParameters = new TokenValidationParameters()
            {
                AllowedAudience = audience,
                SigningToken = new BinarySecretSecurityToken(Convert.FromBase64String(signingKey)),
                ValidIssuer = issuer,
            };

            configuration.AddJsonWebToken(
                validationParameters,
                AuthenticationOptions.ForAuthorizationHeader(JwtConstants.Bearer),
                AuthenticationScheme.SchemeOnly(JwtConstants.Bearer),
                claimMappings);
        }

        public static void AddJsonWebToken(
            this AuthenticationConfiguration configuration,
            string issuer,
            string audience,
            string signingKey,
            string scheme,
            Dictionary<string, string> claimMappings = null)
        {
            var validationParameters = new TokenValidationParameters()
            {
                AllowedAudience = audience,
                SigningToken = new BinarySecretSecurityToken(Convert.FromBase64String(signingKey)),
                ValidIssuer = issuer,
            };

            configuration.AddJsonWebToken(
                validationParameters,
                AuthenticationOptions.ForAuthorizationHeader(scheme),
                AuthenticationScheme.SchemeOnly(scheme),
                claimMappings);
        }

        public static void AddJsonWebToken(
            this AuthenticationConfiguration configuration, 
            string issuer, 
            string audience, 
            X509Certificate2 signingCertificate, 
            Dictionary<string, string> claimMappings = null)
        {
            var validationParameters = new TokenValidationParameters()
            {
                AllowedAudience = audience,
                SigningToken = new X509SecurityToken(signingCertificate),
                ValidIssuer = issuer,
            };

            configuration.AddJsonWebToken(
                validationParameters,
                AuthenticationOptions.ForAuthorizationHeader(JwtConstants.Bearer),
                AuthenticationScheme.SchemeOnly(JwtConstants.Bearer),
                claimMappings);
        }

        public static void AddJsonWebToken(
            this AuthenticationConfiguration configuration,
            string issuer,
            string audience,
            X509Certificate2 signingCertificate,
            string scheme,
            Dictionary<string, string> claimMappings = null)
        {
            var validationParameters = new TokenValidationParameters()
            {
                AllowedAudience = audience,
                SigningToken = new X509SecurityToken(signingCertificate),
                ValidIssuer = issuer,
            };

            configuration.AddJsonWebToken(
                validationParameters,
                AuthenticationOptions.ForAuthorizationHeader(scheme),
                AuthenticationScheme.SchemeOnly(scheme),
                claimMappings);
        }

        public static void AddJsonWebToken(
            this AuthenticationConfiguration configuration, 
            TokenValidationParameters validationParameters,
            AuthenticationOptions options,
            AuthenticationScheme scheme,
            Dictionary<string, string> claimMappings = null)
        {
            var handler = new JwtSecurityTokenHandlerWrapper(validationParameters, claimMappings);

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = options,
                Scheme = scheme
            });
        }

        public static void AddBasicAuthentication(this AuthenticationConfiguration configuration, BasicAuthenticationSecurityTokenHandler.ValidateUserNameCredentialDelegate validationDelegate, string realm = "localhost", bool retainPassword = false)
        {
            var handler = new BasicAuthenticationSecurityTokenHandler(validationDelegate);
            handler.RetainPassword = retainPassword;

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = AuthenticationOptions.ForAuthorizationHeader(scheme: "Basic"),
                Scheme = AuthenticationScheme.SchemeAndRealm("Basic", realm)
            });
        }

        public static void AddBasicAuthentication(this AuthenticationConfiguration configuration, BasicAuthenticationSecurityTokenHandler.ValidateUserNameCredentialDelegate validationDelegate, AuthenticationOptions options, string realm = "localhost", bool retainPassword = false)
        {
            var handler = new BasicAuthenticationSecurityTokenHandler(validationDelegate);
            handler.RetainPassword = retainPassword;

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = options,
                Scheme = AuthenticationScheme.SchemeAndRealm("Basic", realm)
            });
        }

        public static void AddBasicAuthentication(this AuthenticationConfiguration configuration, Func<string, string, bool> validationDelegate, Func<string, string[]> roleDelegate, string realm = "localhost", bool retainPassword = false)
        {
            var handler = new BasicAuthenticationWithRoleSecurityTokenHandler(validationDelegate, roleDelegate);
            handler.RetainPassword = retainPassword;

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = AuthenticationOptions.ForAuthorizationHeader(scheme: "Basic"),
                Scheme = AuthenticationScheme.SchemeAndRealm("Basic", realm)
            });
        }

        public static void AddClientCertificate(this AuthenticationConfiguration configuration, SecurityTokenHandler handler)
        {
            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = AuthenticationOptions.ForClientCertificate()
            });
        }

        public static void AddClientCertificate(this AuthenticationConfiguration configuration, ClientCertificateMode mode, params string[] values)
        {
            var handler = new ClientCertificateHandler(mode, values);

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = AuthenticationOptions.ForClientCertificate()
            });
        }

        private static SecurityTokenHandlerConfiguration CreateSaml2SecurityTokenHandlerConfiguration(string issuerThumbprint, string issuerName, string audienceUri, X509CertificateValidator certificateValidator, X509Certificate2 encryptingCertificate)
        {
            var handlerConfig = CreateSaml2SecurityTokenHandlerConfiguration(issuerThumbprint, issuerName, audienceUri, certificateValidator);

            var serviceTokens = new List<SecurityToken> { new X509SecurityToken(encryptingCertificate) };

            handlerConfig.ServiceTokenResolver =
               SecurityTokenResolver.CreateDefaultSecurityTokenResolver(serviceTokens.AsReadOnly(), true);

            return handlerConfig;
        }

        private static SecurityTokenHandlerConfiguration CreateSaml2SecurityTokenHandlerConfiguration(string issuerThumbprint, string issuerName, string audienceUri, X509CertificateValidator certificateValidator)
        {
            var registry = new ConfigurationBasedIssuerNameRegistry();
            registry.AddTrustedIssuer(issuerThumbprint, issuerName);

            var handlerConfig = new SecurityTokenHandlerConfiguration();
            handlerConfig.AudienceRestriction.AllowedAudienceUris.Add(new Uri(audienceUri));
            handlerConfig.IssuerNameRegistry = registry;
            handlerConfig.CertificateValidator = certificateValidator;
            return handlerConfig;
        }

        public static void AddSaml2(this AuthenticationConfiguration configuration, string issuerThumbprint, string issuerName, string audienceUri, X509CertificateValidator certificateValidator, AuthenticationOptions options, AuthenticationScheme scheme, X509Certificate2 encryptingCertificate)
        {
            var handlerConfig = CreateSaml2SecurityTokenHandlerConfiguration(issuerThumbprint, issuerName, audienceUri, certificateValidator, encryptingCertificate);

            configuration.AddSaml2(handlerConfig, options, scheme);
        }

        public static void AddSaml2(this AuthenticationConfiguration configuration, string issuerThumbprint, string issuerName, string audienceUri, X509CertificateValidator certificateValidator, AuthenticationOptions options, AuthenticationScheme scheme)
        {
            var handlerConfig = CreateSaml2SecurityTokenHandlerConfiguration(issuerThumbprint, issuerName, audienceUri, certificateValidator);

            configuration.AddSaml2(handlerConfig, options, scheme);
        }

        public static void AddSaml2(this AuthenticationConfiguration configuration, SecurityTokenHandlerConfiguration handlerConfiguration, AuthenticationOptions options, AuthenticationScheme scheme)
        {
            var handler = new HttpSaml2SecurityTokenHandler();
            handler.Configuration = handlerConfiguration;

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = options,
                Scheme = scheme
            });
        }

        public static void AddSaml2AndJwt(this AuthenticationConfiguration configuration, string issuerThumbprint, X509Certificate2 signingCertificate, string issuerName, string audienceUri, X509CertificateValidator certificateValidator, AuthenticationOptions options, AuthenticationScheme scheme, X509Certificate2 encryptionCertificate)
        {
            var validationParameters = new TokenValidationParameters()
            {
                AllowedAudience = audienceUri,
                SigningToken = new X509SecurityToken(signingCertificate),
                ValidIssuer = issuerName,
            };

            var jwtHandler = new JwtSecurityTokenHandlerWrapper(validationParameters);

            var samlHandlerConfig = CreateSaml2SecurityTokenHandlerConfiguration(issuerThumbprint, issuerName, audienceUri, certificateValidator, encryptionCertificate);
            var saml2Handler = new HttpSaml2SecurityTokenHandler() { Configuration = samlHandlerConfig };

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { jwtHandler, saml2Handler },
                Options = options,
                Scheme = scheme
            });
        }

        public static void AddSaml11(this AuthenticationConfiguration configuration, SecurityTokenHandlerConfiguration handlerConfiguration, AuthenticationOptions options)
        {
            var handler = new HttpSamlSecurityTokenHandler();
            handler.Configuration = handlerConfiguration;

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = options
            });
        }

        // todo: think about integration strategy
        //public static void AddHawkAuthentication(this AuthenticationConfiguration configuration, Func<string, Credential> credentialsCallback, bool allowBewit = false, Func<HttpResponseMessage, string> normalizationCallback = null, Func<HttpRequestMessage, string, bool> verificationCallback = null)
        //{
        //    var handler = new HawkSecurityTokenHandler(
        //                    new HawkAuthenticationHandler(credentialsCallback,
        //                                                    normalizationCallback, verificationCallback));

        //    configuration.AddMapping(new AuthenticationOptionMapping
        //    {
        //        TokenHandler = new SecurityTokenHandlerCollection { handler },
        //        Options = AuthenticationOptions.ForAuthorizationHeader(scheme: "hawk"),
        //        Scheme = AuthenticationScheme.SchemeOnly("hawk")
        //    });

        //    if (allowBewit)
        //    {
        //        configuration.AddMapping(new AuthenticationOptionMapping
        //        {
        //            TokenHandler = new SecurityTokenHandlerCollection { handler },
        //            Options = AuthenticationOptions.ForQueryString("bewit")
        //        });
        //    }
        //}
    }
}
