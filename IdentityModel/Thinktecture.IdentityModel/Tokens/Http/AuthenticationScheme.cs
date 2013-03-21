using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityModel.Tokens.Http
{
    public class AuthenticationScheme
    {
        public string Scheme { get; set; }
        public string Challenge { get; set; }

        public static AuthenticationScheme SchemeOnly(string scheme)
        {
            return new AuthenticationScheme
            {
                Scheme = scheme,
            };
        }

        public static AuthenticationScheme SchemeAndChallenge(string scheme, string challenge)
        {
            return new AuthenticationScheme
            {
                Scheme = scheme,
                Challenge = challenge
            };
        }

        public static AuthenticationScheme SchemeAndRealm(string scheme, string realm)
        {
            return new AuthenticationScheme
            {
                Scheme = scheme,
                Challenge = string.Format("realm=\"{0}\"", realm)
            };
        }
    }
}
