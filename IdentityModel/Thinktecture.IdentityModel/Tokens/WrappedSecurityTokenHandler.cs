using System;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens.Http;

namespace Thinktecture.IdentityModel.Tokens
{
    internal class WrappedSecurityTokenHandler<T> : SecurityTokenHandler where T : PluggableDelegatingHandler
    {
        T handler;

        public T Handler
        {
            get
            {
                return handler;
            }
        }

        public WrappedSecurityTokenHandler(T handler)
        {
            this.handler = handler;
        }
      
        public override string[] GetTokenTypeIdentifiers()
        {
            return new[] { String.Empty };
        }

        public override Type TokenType
        {
            get { return null; }
        }
    }
}
