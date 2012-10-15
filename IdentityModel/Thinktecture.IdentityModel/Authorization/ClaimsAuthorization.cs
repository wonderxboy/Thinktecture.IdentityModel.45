/*
 * Copyright (c) Dominick Baier.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IdentityModel.Services;
using System.Linq;
using System.Security.Claims;

namespace Thinktecture.IdentityModel.Authorization
{
    /// <summary>
    /// Provides direct access methods for evaluating authorization policy
    /// </summary>
    public static class ClaimsAuthorization
    {
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

        /// <summary>
        /// Gets the registered authorization manager.
        /// </summary>
        public static ClaimsAuthorizationManager AuthorizationManager
        {
            get
            {
                return FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthorizationManager;
            }
        }

        /// <summary>
        /// Checks the authorization policy.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="action">The action.</param>
        /// <returns>true when authorized, otherwise false</returns>
        public static bool CheckAccess(string action, string resource)
        {
            Contract.Requires(!String.IsNullOrEmpty(resource));
            Contract.Requires(!String.IsNullOrEmpty(action));


            return CheckAccess(ClaimsPrincipal.Current, action, resource);
        }

        public static bool CheckAccess(string action, string resource, params string[] additionalResources)
        {
            return CheckAccess(
                ClaimsPrincipal.Current,
                action,
                resource,
                additionalResources);
        }

        /// <summary>
        /// Checks the authorization policy.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <param name="action">The action.</param>
        /// <param name="resource">The resource.</param>
        /// <returns>true when authorized, otherwise false</returns>
        public static bool CheckAccess(ClaimsPrincipal principal, string action, string resource)
        {
            Contract.Requires(!String.IsNullOrEmpty(resource));
            Contract.Requires(!String.IsNullOrEmpty(action));
            Contract.Requires(principal != null);


            var context = new AuthorizationContext(principal, resource, action);

            return AuthorizationManager.CheckAccess(context);
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

        /// <summary>
        /// Checks the authorization policy.
        /// </summary>
        /// <param name="actions">The actions.</param>
        /// <param name="resources">The resources.</param>
        /// <returns>true when authorized, otherwise false</returns>
        public static bool CheckAccess(Collection<Claim> actions, Collection<Claim> resources)
        {
            Contract.Requires(actions != null);
            Contract.Requires(resources != null);


            return CheckAccess(new AuthorizationContext(
                ClaimsPrincipal.Current, resources, actions));
        }

        /// <summary>
        /// Checks the authorization policy.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <returns>true when authorized, otherwise false</returns>
        public static bool CheckAccess(AuthorizationContext context)
        {
            Contract.Requires(context != null);


            return AuthorizationManager.CheckAccess(context);
        }

        public static AuthorizationContext CreateAuthorizationContext(ClaimsPrincipal principal, string action, string resource, params string[] additionalResources)
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
