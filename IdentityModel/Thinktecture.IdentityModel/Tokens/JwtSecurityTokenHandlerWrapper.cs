using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Claims;
using System.Xml;
using Thinktecture.IdentityModel.Constants;
using Thinktecture.IdentityModel.Http;

namespace Thinktecture.IdentityModel.Tokens
{
    public class JwtSecurityTokenHandlerWrapper : JwtSecurityTokenHandler
    {
        TokenValidationParameters validationParams;

        public JwtSecurityTokenHandlerWrapper(TokenValidationParameters validationParams, Dictionary<string, string> inboundClaimTypeMap = null)
        {
            this.validationParams = validationParams;

            if (inboundClaimTypeMap != null)
            {
                InboundClaimTypeMap = inboundClaimTypeMap;
            }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<System.Security.Claims.ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            var jwt = token as JwtSecurityToken;
            var list = new List<ClaimsIdentity>(this.ValidateToken(jwt, validationParams).Identities);
            return list.AsReadOnly();
        }

        public override bool CanReadToken(string jwtEncodedString)
        {
            // unbase64 header if necessary
            if (HeaderEncoding.IsBase64Encoded(jwtEncodedString))
            {
                jwtEncodedString = HeaderEncoding.DecodeBase64(jwtEncodedString);
            }

            if (jwtEncodedString.StartsWith("<"))
            {
                return base.CanReadToken(new XmlTextReader(new StringReader(jwtEncodedString)));
            }

            return base.CanReadToken(jwtEncodedString);
        }

        public override SecurityToken ReadToken(string jwtEncodedString)
        {
            // unbase64 header if necessary
            if (HeaderEncoding.IsBase64Encoded(jwtEncodedString))
            {
                jwtEncodedString = HeaderEncoding.DecodeBase64(jwtEncodedString);
            }

            if (jwtEncodedString.StartsWith("<"))
            {
                return base.ReadToken(new XmlTextReader(new StringReader(jwtEncodedString)));
            }

            return base.ReadToken(jwtEncodedString);
        }
    }
}
