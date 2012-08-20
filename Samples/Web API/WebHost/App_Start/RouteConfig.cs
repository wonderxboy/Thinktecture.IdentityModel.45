using Resources.Security;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Thinktecture.IdentityModel.Tokens.Http;

namespace WebApiSecurity
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // default API route
            routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            
            // default API route with claims transformation
            routes.MapHttpRoute(
                name: "DefaultApiWithTransformation",
                routeTemplate: "api2/identity",
                defaults: new { controller = "Identity" } ,
                constraints: null,
                handler: new ClaimsTransformationHandler(new ConsultantsClaimsTransformer(), GlobalConfiguration.Configuration)
            );

            // default MVC route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}