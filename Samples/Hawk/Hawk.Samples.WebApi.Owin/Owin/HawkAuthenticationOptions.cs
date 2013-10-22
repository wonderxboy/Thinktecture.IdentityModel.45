using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;

namespace Thinktecture.IdentityModel.Http.Hawk.Owin
{
    public class HawkAuthenticationOptions : AuthenticationOptions
    {
        public HawkAuthenticationOptions(Options hawkOptions) : base(HawkConstants.Scheme)
        {
            this.HawkOptions = hawkOptions;
        }

        public Options HawkOptions { get; set; }
    }
}
