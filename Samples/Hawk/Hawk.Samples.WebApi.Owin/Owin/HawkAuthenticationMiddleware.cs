using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityModel.Http.Hawk.Owin
{
    public class HawkAuthenticationMiddleware : AuthenticationMiddleware<HawkAuthenticationOptions>
    {
        public HawkAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, HawkAuthenticationOptions options)
            : base(next, options)
        { }

        protected override AuthenticationHandler<HawkAuthenticationOptions> CreateHandler()
        {
            return new HawkAuthenticationHandler();
        }
    }
}
