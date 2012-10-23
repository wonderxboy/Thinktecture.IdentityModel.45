using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Security;
using Thinktecture.IdentityModel.Tokens.Http;

namespace FormsAndBasicAuth
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // configure basic auth
            var authConfig = new AuthenticationConfiguration
            {
                DefaultAuthenticationScheme = "Basic"
            };

            authConfig.AddBasicAuthentication((userName, password) => Membership.ValidateUser(userName, password));
            config.MessageHandlers.Add(new AuthenticationHandler(authConfig));
        }
    }
}
