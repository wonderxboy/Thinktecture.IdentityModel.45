using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Tokens;

namespace Thinktecture.IdentityModel.Http.Hawk.WebApi
{
    internal class HawkSecurityTokenHandler : WrappedSecurityTokenHandler<HawkAuthenticationHandler>
    {
        public HawkSecurityTokenHandler(HawkAuthenticationHandler handler) : base(handler) { }
    }
}
