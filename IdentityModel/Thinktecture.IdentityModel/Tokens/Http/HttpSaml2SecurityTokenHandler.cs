/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.IdentityModel.Tokens;
using System.IO;
using System.Xml;
using Thinktecture.IdentityModel.Constants;
using Thinktecture.IdentityModel.Http;

namespace Thinktecture.IdentityModel.Tokens.Http
{
    public class HttpSaml2SecurityTokenHandler : Saml2SecurityTokenHandler
    {
        private string[] _identifier = new string[] 
            { 
                "Saml2",
                TokenTypes.OasisWssSaml2TokenProfile11,
                TokenTypes.Saml2TokenProfile11
            };

        public HttpSaml2SecurityTokenHandler()
            : base()
        { }

        public HttpSaml2SecurityTokenHandler(string identifier)
            : base()
        {
            _identifier = new string[] { identifier };
        }

        public HttpSaml2SecurityTokenHandler(SamlSecurityTokenRequirement requirement, string identifier)
            : base(requirement)
        {
            _identifier = new string[] { identifier };
        }

        public override SecurityToken ReadToken(string tokenString)
        {
            // unbase64 header if necessary
            if (HeaderEncoding.IsBase64Encoded(tokenString))
            {
                tokenString = HeaderEncoding.DecodeBase64(tokenString);
            }

            return ReadToken(new XmlTextReader(new StringReader(tokenString)));
        }

        public override bool CanReadToken(string tokenString)
        {
            // unbase64 header if necessary
            if (HeaderEncoding.IsBase64Encoded(tokenString))
            {
                tokenString = HeaderEncoding.DecodeBase64(tokenString);
            }

            if (tokenString.StartsWith("<"))
            {
                return base.CanReadToken(new XmlTextReader(new StringReader(tokenString)));
            }

            return base.CanReadToken(tokenString);
        }

        public override string[] GetTokenTypeIdentifiers()
        {
            return _identifier;
        }
    }
}