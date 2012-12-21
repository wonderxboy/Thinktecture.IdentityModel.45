using System.Web.Http;
using Thinktecture.IdentityModel.Tokens.Http;

namespace Thinktecture.Samples.Security
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var authentication = CreateAuthenticationConfiguration();

            // route to identity controller
            config.Routes.MapHttpRoute(
                name: "Identity",
                routeTemplate: "api/identity",
                defaults: new { controller = "identity" },
                constraints: null,
                handler: new AuthenticationHandler(authentication, config)
            );
        }

        private static AuthenticationConfiguration CreateAuthenticationConfiguration()
        {
            var authentication = new AuthenticationConfiguration
            {
                InheritHostClientIdentity = true,
                DefaultAuthenticationScheme = "Basic"
            };

            authentication.AddBasicAuthentication((username, password) 
                => UserCredentials.Validate(username, password));

            return authentication;
        }
    }
}