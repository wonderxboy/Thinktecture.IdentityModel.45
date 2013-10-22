using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Thinktecture.IdentityModel.Web.Mvc
{
    /// <summary>
    /// Indicate the scenarios in which the response may be hosted in an iframe. See https://developer.mozilla.org/en-US/docs/HTTP/X-Frame-Options for more information.
    /// </summary>
    public enum FrameOptions
    {
        /// <summary>
        /// Response is never allowed in an iframe.
        /// </summary>
        Deny,
        // Response is only allowed in an iframe if the hosting page is from the same origin.
        SameOrigin,
        // Response is only allowed in an iframe is the hosting page is from the specified origin.
        CustomOrigin
    }

    /// <summary>
    ///  FrameOptionsAttribute allows you to set the X-Frame-Options HTTP response header to prevent clickjacking attacks. See https://developer.mozilla.org/en-US/docs/HTTP/X-Frame-Options for more information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple=false)]
    public class FrameOptionsAttribute : ActionFilterAttribute
    {
        const string HeaderName = "X-Frame-Options";
        const string Deny = "DENY";
        const string SameOrigin = "SAMEORIGIN";
        const string AllowFrom = "ALLOW-FROM {0}";

        string customOrigin;
        FrameOptions options;
        
        /// <summary>
        /// This default ctor uses FrameOptions.Deny.
        /// </summary>
        public FrameOptionsAttribute()
        {
            this.options = FrameOptions.Deny;
        }

        /// <summary>
        /// Use this ctor to specify the FrameOptions.
        /// </summary>
        /// <param name="options">FrameOptions to use.</param>
        public FrameOptionsAttribute(FrameOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Use this ctor to specify a custom origin.
        /// </summary>
        /// <param name="origin">The origin to allow.</param>
        public FrameOptionsAttribute(string origin)
        {
            if (String.IsNullOrWhiteSpace(origin)) throw new ArgumentNullException("origin");

            this.options = FrameOptions.CustomOrigin;
            this.customOrigin = origin;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext == null) throw new ArgumentNullException("filterContext");

            if (filterContext.IsChildAction) return;

            string value = null;
            switch (this.options)
            {   
                case FrameOptions.Deny:
                    value = Deny;
                    break;
                case FrameOptions.SameOrigin:
                    value = SameOrigin;
                    break;
                case FrameOptions.CustomOrigin:
                    value = GetCustomOriginHeaderValue(filterContext.HttpContext.Request);
                    break;
                default:
                    throw new Exception("Invalid FrameOptions");
            }

            filterContext.HttpContext.Response.AddHeader(HeaderName, value);
        }

        string GetCustomOriginHeaderValue(HttpRequestBase request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var origin = this.GetCustomOrigin(request);
            if (String.IsNullOrWhiteSpace(origin))
            {
                // if they return null, we default to safe mode
                return Deny;
            }
            return String.Format(AllowFrom, origin);
        }
        
        protected virtual string GetCustomOrigin(HttpRequestBase request)
        {
            return this.customOrigin;
        }
    }
}
