using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;

namespace Thinktecture.IdentityModel.Authorization.Mvc
{
    public class ClaimsAuthorizeAttribute : AuthorizeAttribute
    {
        private string _resource;
        private string _action;
        private string[] _additionalResources;

        private const string _label = "Thinktecture.IdentityModel.Authorization.Mvc.ClaimsAuthorizeAttribute";

        public ClaimsAuthorizeAttribute()
        { }

        public ClaimsAuthorizeAttribute(string action, string resource, params string[] additionalResources)
        {
            _action = action;
            _resource = resource;
            _additionalResources = additionalResources;
        }

        public override void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
        {
            filterContext.HttpContext.Items[_label] = filterContext;
            base.OnAuthorization(filterContext); 
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!string.IsNullOrWhiteSpace(_action) && !string.IsNullOrWhiteSpace(_resource))
            {
                return ClaimsAuthorization.CheckAccess(_action, _resource, _additionalResources);
            }
            else
            {
                var filterContext = httpContext.Items[_label] as System.Web.Mvc.AuthorizationContext;
                return CheckAccess(filterContext);
            }
        }

        protected virtual bool CheckAccess(System.Web.Mvc.AuthorizationContext filterContext)
        {
            var action = filterContext.RouteData.Values["action"] as string;
            var controller = filterContext.RouteData.Values["controller"] as string;

            return ClaimsAuthorization.CheckAccess(action, controller);
        }
    }
}
