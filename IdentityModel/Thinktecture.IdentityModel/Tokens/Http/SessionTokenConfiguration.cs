/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.IdentityModel.Tokens;

namespace Thinktecture.IdentityModel.Tokens.Http
{
    public class SessionTokenConfiguration
    {
        //JwtSecurityTokenHandler _handler;
        //object _handlerLock = new object();

        public TimeSpan DefaultTokenLifetime { get; set; }
        public string EndpointAddress { get; set; }
        public string HeaderName { get; set; }
        public string Scheme { get; set; }
        public string Audience { get; set; }
        public byte[] SigningKey { get; set; }
        public string IssuerName { get; set; }
        public TokenValidationParameters ValidationParameters { get; set; }

        //public JwtSecurityTokenHandler SecurityTokenHandler
        //{
        //    get
        //    {
        //        if (_handler == null)
        //        {
        //            lock (_handlerLock)
        //            {
        //                if (_handler == null)
        //                {
        //                    var config = new SecurityTokenHandlerConfiguration();
        //                    var registry = new WebTokenIssuerNameRegistry();
        //                    registry.AddTrustedIssuer(IssuerName, IssuerName);
        //                    config.IssuerNameRegistry = registry;

        //                    var issuerResolver = new WebTokenIssuerTokenResolver();
        //                    issuerResolver.AddSigningKey(IssuerName, SigningKey);
        //                    config.IssuerTokenResolver = issuerResolver;

        //                    config.AudienceRestriction.AllowedAudienceUris.Add(Audience);

        //                    var handler = new JsonWebTokenHandler();
        //                    handler.Configuration = config;

        //                    _handler = handler;
        //                }
        //            }
        //        }

        //        return _handler;
        //    }
        //}

        public SessionTokenConfiguration()
        {
            DefaultTokenLifetime = TimeSpan.FromHours(10);
            EndpointAddress = "/token";
            HeaderName = "Authorization";
            Scheme = "Session";
            Audience = "http://session.tt";
            IssuerName = "session issuer";
            SigningKey = CryptoRandom.CreateRandomKey(32);
        }
    }
}
