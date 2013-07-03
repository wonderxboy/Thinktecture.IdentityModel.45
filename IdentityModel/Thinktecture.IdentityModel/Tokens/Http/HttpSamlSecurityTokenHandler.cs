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
    class HttpSamlSecurityTokenHandler : SamlSecurityTokenHandler
    {
        private string[] _identifier = new string[] 
            { 
                "Saml",
                TokenTypes.OasisWssSaml11TokenProfile11,
                TokenTypes.Saml11TokenProfile11
            };


        public HttpSamlSecurityTokenHandler()
            : base()
        { }

        public HttpSamlSecurityTokenHandler(string identifier)
            : base()
        {
            _identifier = new string[] { identifier };
        }

        public HttpSamlSecurityTokenHandler(SamlSecurityTokenRequirement requirement)
            : base(requirement)
        { }

        public HttpSamlSecurityTokenHandler(SamlSecurityTokenRequirement requirement, string identifier)
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

            return ContainingCollection.ReadToken(new XmlTextReader(new StringReader(tokenString)));
        }

        public override string[] GetTokenTypeIdentifiers()
        {
            return _identifier;
        }
    }
}
