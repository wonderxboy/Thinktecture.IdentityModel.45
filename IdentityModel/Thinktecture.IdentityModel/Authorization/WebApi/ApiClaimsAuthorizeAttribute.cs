using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Thinktecture.IdentityModel.Authorization.WebApi
{
    public class ApiClaimsAuthorizeAttribute : AuthorizeAttribute
    {
        private string _resource;
        private string _action;
        private string[] _additionalResources;

        /// <summary>
        /// Default action claim type.
        /// </summary>
        public const string ActionType = "http://application/claims/authorization/action";

        /// <summary>
        /// Default resource claim type
        /// </summary>
        public const string ResourceType = "http://application/claims/authorization/resource";

        /// <summary>
        /// Additional resource claim type
        /// </summary>
        public const string AdditionalResourceType = "http://application/claims/authorization/additionalresource";

        public static bool CheckAccess(string action, string resource, params string[] additionalResources)
        {
            return CheckAccess(
                ClaimsPrincipal.Current,
                action,
                resource,
                additionalResources);
        }

        public static bool CheckAccess(ClaimsPrincipal principal, string action, string resource, params string[] additionalResources)
        {
            var context = CreateAuthorizationContext(
                principal,
                action,
                resource,
                additionalResources);

            return ClaimsAuthorization.CheckAccess(context);
        }

        public ApiClaimsAuthorizeAttribute()
        { }

        public ApiClaimsAuthorizeAttribute(string action, string resource, params string[] additionalResources)
        {
            _action = action; 
            _resource = resource;
            _additionalResources = additionalResources;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (!string.IsNullOrWhiteSpace(_action) && !string.IsNullOrWhiteSpace(_resource))
            {
                return CheckAccess(_action, _resource, _additionalResources);
            }
            else
            {
                return CheckAccess(actionContext);
            }
        }

        protected virtual bool CheckAccess(HttpActionContext actionContext)
        {
            var action = actionContext.ActionDescriptor.ActionName;
            var resource = actionContext.ControllerContext.ControllerDescriptor.ControllerName;

            return CheckAccess(
                ClaimsPrincipal.Current,
                action,
                resource);
        }

        private static AuthorizationContext CreateAuthorizationContext(ClaimsPrincipal principal, string action, string resource, params string[] additionalResources)
        {
            var actionClaims = new Collection<Claim>
            {
                new Claim(ActionType, action)
            };

            var resourceClaims = new Collection<Claim>
            {
                new Claim(ResourceType, resource)
            };

            if (additionalResources != null && additionalResources.Length > 0)
            {
                additionalResources.ToList().ForEach(ar => resourceClaims.Add(new Claim(AdditionalResourceType, ar)));
            }

            return new AuthorizationContext(
                principal,
                resourceClaims,
                actionClaims);
        }
    }
}
