using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityModel.Http.Hawk.Owin.Extensions
{
    public static class HawkAuthenticationExtension
    {
        public static IAppBuilder UseHawkAuthentication(this IAppBuilder app, HawkAuthenticationOptions options)
        {
            app.Use(typeof(HawkAuthenticationMiddleware), app, options);
            app.UseStageMarkerAuthenticate();
            return app;
        }
    }
}
