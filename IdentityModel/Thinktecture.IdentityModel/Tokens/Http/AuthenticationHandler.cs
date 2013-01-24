/*
 * Copyright (c) Dominick Baier.  All rights reserved.
 * see license.txt
 */

using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Thinktecture.IdentityModel.Diagnostics;

namespace Thinktecture.IdentityModel.Tokens.Http
{
    public class AuthenticationHandler : DelegatingHandler
    {
        HttpAuthentication _authN;

        public AuthenticationHandler(AuthenticationConfiguration configuration, HttpConfiguration httpConfiguration = null)
        {
            _authN = new HttpAuthentication(configuration);

            if (httpConfiguration != null)
            {
                InnerHandler = new HttpControllerDispatcher(httpConfiguration);
            }
        }

        public AuthenticationHandler(AuthenticationConfiguration configuration, HttpMessageHandler innerHandler)
        {
            _authN = new HttpAuthentication(configuration);
            InnerHandler = innerHandler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Tracing.Start(Area.HttpAuthentication);

            // check SSL requirement
            if (_authN.Configuration.RequireSsl)
            {
                if (request.RequestUri.Scheme != Uri.UriSchemeHttps)
                {
                    Tracing.Information(Area.HttpAuthentication, "Request rejected because it is not over HTTPS.");

                    var forbiddenResponse =
                        request.CreateResponse(HttpStatusCode.Forbidden);

                    forbiddenResponse.ReasonPhrase = "HTTPS Required.";
                    return forbiddenResponse;
                }
            }

            // check if reuse of host client identity is allowed
            if (_authN.Configuration.InheritHostClientIdentity == false)
            {
                Tracing.Verbose(Area.HttpAuthentication, "Host client identity is not inherited. Setting anonymous principal");
                SetPrincipal(Principal.Anonymous);
            }

            ClaimsPrincipal principal;
            try
            {
                // try to authenticate
                // returns an anonymous principal if no credential was found
                principal = _authN.Authenticate(request);

                if (principal == null)
                {
                    // this should never return null - check the corresponding handler!
                    Tracing.Error(Area.HttpAuthentication, "Authentication returned null principal. Something is wrong!");
                    return SendUnauthorizedResponse(request);
                }
            }
            catch (AuthenticationException aex)
            {
                // a handler wants to send back a specific error response
                return SendAuthenticationExceptionResponse(aex, request);
            }
            catch (Exception ex)
            {
                // something went wrong during authentication (e.g. invalid credentials)
                Tracing.Error(Area.HttpAuthentication, "Exception while validating the token: " + ex.ToString());
                return SendUnauthorizedResponse(request);
            }

            // credential was found *and* authentication was successful
            if (principal.Identity.IsAuthenticated)
            {
                Tracing.Verbose(Area.HttpAuthentication, "Authentication successful.");

                // check for token request - if yes send token back and return
                if (_authN.IsSessionTokenRequest(request))
                {
                    Tracing.Information(Area.HttpAuthentication, "Request for session token.");
                    return SendSessionTokenResponse(principal, request);
                }

                // else set the principal
                SetPrincipal(principal);
            }

            // call service code
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                SetAuthenticateHeader(response);
            }

            return response;
        }

        private HttpResponseMessage SendAuthenticationExceptionResponse(AuthenticationException aex, HttpRequestMessage request)
        {
            var response = request.CreateResponse(aex.StatusCode);
            response.ReasonPhrase = aex.ReasonPhrase;

            if (aex.StatusCode == HttpStatusCode.Unauthorized)
            {
                SetAuthenticateHeader(response);
            }
            
            return response;
        }

        private HttpResponseMessage SendUnauthorizedResponse(HttpRequestMessage request)
        {
            var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);

            SetAuthenticateHeader(unauthorizedResponse);
            unauthorizedResponse.ReasonPhrase = "Unauthorized.";

            return unauthorizedResponse;
        }

        private HttpResponseMessage SendSessionTokenResponse(ClaimsPrincipal principal, HttpRequestMessage request)
        {
            var tokenResponse = _authN.CreateSessionTokenResponse(principal);

            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(tokenResponse, Encoding.UTF8, "application/json");

            return response;
        }

        protected virtual void SetAuthenticateHeader(HttpResponseMessage response, string scheme = null)
        {
            if (_authN.Configuration.SendWwwAuthenticateResponseHeader)
            {
                AuthenticationHeaderValue header;
                if (!string.IsNullOrWhiteSpace(scheme))
                {
                    header = new AuthenticationHeaderValue(scheme);
                }
                else
                {
                    header = new AuthenticationHeaderValue(_authN.Configuration.DefaultAuthenticationScheme);
                }

                Tracing.Verbose(Area.HttpAuthentication, "Setting Www-Authenticate header with scheme: " + header.Scheme);
                response.Headers.WwwAuthenticate.Add(header);
            }
        }

        protected virtual void SetPrincipal(ClaimsPrincipal principal)
        {
            if (principal.Identity.IsAuthenticated)
            {
                string name = "unknown";

                if (!string.IsNullOrWhiteSpace(principal.Identity.Name))
                {
                    name = principal.Identity.Name;
                }
                else if (principal.Claims.First() != null)
                {
                    name = principal.Claims.First().Value;
                }

                Tracing.Verbose(Area.HttpAuthentication, "Authentication successful for: " + name);
            }
            else
            {
                Tracing.Verbose(Area.HttpAuthentication, "Setting anonymous principal.");
            }

            Thread.CurrentPrincipal = principal;

            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }
    }
}