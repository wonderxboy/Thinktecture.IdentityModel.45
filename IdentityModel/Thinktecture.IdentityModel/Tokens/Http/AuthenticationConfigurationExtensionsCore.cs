/*
 * Copyright (c) Dominick Baier.  All rights reserved.
 * see license.txt
 */

using System;
using System.IdentityModel.Tokens;

namespace Thinktecture.IdentityModel.Tokens.Http
{
    public static class AuthenticationConfigurationExtensionsCore
    {
        public static void AddAccessKey(this AuthenticationConfiguration configuration, SimpleSecurityTokenHandler handler, AuthenticationOptions options)
        {
            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = options
            });
        }

        public static void AddAccessKey(this AuthenticationConfiguration configuration, SimpleSecurityTokenHandler.ValidateTokenDelegate validateTokenDelegate, AuthenticationOptions options)
        {
            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { new SimpleSecurityTokenHandler(validateTokenDelegate) },
                Options = options
            });
        }

        public static void AddSimpleWebToken(this AuthenticationConfiguration configuration, string issuer, string audience, string signingKey, AuthenticationOptions options)
        {
            var config = new SecurityTokenHandlerConfiguration();
            var registry = new WebTokenIssuerNameRegistry();
            registry.AddTrustedIssuer(issuer, issuer);
            config.IssuerNameRegistry = registry;

            var issuerResolver = new WebTokenIssuerTokenResolver();
            issuerResolver.AddSigningKey(issuer, signingKey);
            config.IssuerTokenResolver = issuerResolver;

            config.AudienceRestriction.AllowedAudienceUris.Add(new Uri(audience));

            var handler = new SimpleWebTokenHandler();
            handler.Configuration = config;

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = options
            });
        }

        public static void AddJsonWebToken(this AuthenticationConfiguration configuration, string issuer, string audience, string signingKey, AuthenticationOptions options)
        {
            var config = new SecurityTokenHandlerConfiguration();
            var registry = new WebTokenIssuerNameRegistry();
            registry.AddTrustedIssuer(issuer, issuer);
            config.IssuerNameRegistry = registry;

            var issuerResolver = new WebTokenIssuerTokenResolver();
            issuerResolver.AddSigningKey(issuer, signingKey);
            config.IssuerTokenResolver = issuerResolver;

            config.AudienceRestriction.AllowedAudienceUris.Add(new Uri(audience));

            var handler = new JsonWebTokenHandler();
            handler.Configuration = config;

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = options
            });
        }

        public static void AddBasicAuthentication(this AuthenticationConfiguration configuration, BasicAuthenticationSecurityTokenHandler.ValidateUserNameCredentialDelegate validationDelegate, bool retainPassword = false)
        {
            var handler = new BasicAuthenticationSecurityTokenHandler(validationDelegate);
            handler.RetainPassword = retainPassword;

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = AuthenticationOptions.ForAuthorizationHeader(scheme: "Basic")
            });
        }

        public static void AddBasicAuthentication(this AuthenticationConfiguration configuration, BasicAuthenticationSecurityTokenHandler.ValidateUserNameCredentialDelegate validationDelegate, AuthenticationOptions options, bool retainPassword = false)
        {
            var handler = new BasicAuthenticationSecurityTokenHandler(validationDelegate);
            handler.RetainPassword = retainPassword;

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = options
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

        public static void AddSaml2(this AuthenticationConfiguration configuration, SecurityTokenHandlerConfiguration handlerConfiguration, AuthenticationOptions options)
        {
            var handler = new HttpSaml2SecurityTokenHandler();
            handler.Configuration = handlerConfiguration;

            configuration.AddMapping(new AuthenticationOptionMapping
            {
                TokenHandler = new SecurityTokenHandlerCollection { handler },
                Options = options
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
    }
}
