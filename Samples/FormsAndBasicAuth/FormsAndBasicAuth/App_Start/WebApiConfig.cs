using System.IdentityModel.Services;
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

            var authConfig = new AuthenticationConfiguration
            {
                InheritHostClientIdentity = true,
                ClaimsAuthenticationManager = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager
            };

            // setup authentication against membership
            authConfig.AddBasicAuthentication((userName, password) => Membership.ValidateUser(userName, password));
            
            config.MessageHandlers.Add(new AuthenticationHandler(authConfig));
        }
    }
}
