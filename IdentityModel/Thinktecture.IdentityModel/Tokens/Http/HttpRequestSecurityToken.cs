using System;
using System.IdentityModel.Tokens;
using System.Net.Http;

namespace Thinktecture.IdentityModel.Tokens.Http
{
    public class HttpRequestSecurityToken : SecurityToken
    {
        HttpRequestMessage _request;

        public HttpRequestMessage Request 
        {
            get
            {
                return _request;
            }
        }

        public HttpRequestSecurityToken(HttpRequestMessage request)
        {
            _request = request;
        }

        #region Not Implemented
        public override string Id
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { throw new NotImplementedException(); }
        }

        public override DateTime ValidFrom
        {
            get { throw new NotImplementedException(); }
        }

        public override DateTime ValidTo
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
