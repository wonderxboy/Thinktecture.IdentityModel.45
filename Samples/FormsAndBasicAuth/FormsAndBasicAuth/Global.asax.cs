﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebMatrix.WebData;

namespace FormsAndBasicAuth
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

            WebSecurity.InitializeDatabaseConnection("AccountConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
            
            if (!WebSecurity.UserExists("admin"))
            { 
                WebSecurity.CreateUserAndAccount("admin", "admin"); 
            }
        }

        protected void Application_PostAuthenticateRequest()
        {
            if (ClaimsPrincipal.Current.Identity.IsAuthenticated)
            {
                var transformer = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager;
                var newPrincipal = transformer.Authenticate(string.Empty, ClaimsPrincipal.Current);

                Thread.CurrentPrincipal = newPrincipal;
                HttpContext.Current.User = newPrincipal;
            }
        }
    }
}