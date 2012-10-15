using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Thinktecture.IdentityModel.Authorization.WebApi
{
    public class ClaimsAuthorizeAttribute : AuthorizeAttribute
    {
        private string _resource;
        private string _action;
        private string[] _additionalResources;

        public ClaimsAuthorizeAttribute()
        { }

        public ClaimsAuthorizeAttribute(string action, string resource, params string[] additionalResources)
        {
            _action = action; 
            _resource = resource;
            _additionalResources = additionalResources;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (!string.IsNullOrWhiteSpace(_action) && !string.IsNullOrWhiteSpace(_resource))
            {
                return ClaimsAuthorization.CheckAccess(_action, _resource, _additionalResources);
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

            return ClaimsAuthorization.CheckAccess(
                action,
                resource);
        }
    }
}
