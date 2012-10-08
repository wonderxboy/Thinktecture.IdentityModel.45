using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebApiInAppWindows.Security;

namespace WebApiInAppWindows
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // one possible way to wrap the principal
        //protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        //{
        //    var principal = new CustomClaimsPrincipal(ClaimsPrincipal.Current);

        //    HttpContext.Current.User = principal;
        //    Thread.CurrentPrincipal = principal;
        //}
    }
}