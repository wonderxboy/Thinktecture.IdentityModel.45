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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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
                    return Task.FromResult<HttpResponseMessage>(forbiddenResponse);
                }
            }

            // check if reuse of host client identity is allowed
            if (_authN.Configuration.InheritHostClientIdentity == false)
            {
                Tracing.Verbose(Area.HttpAuthentication, "Host client identity is not inherited. Setting anonymous principal");
                SetPrincipal(Principal.Anonymous);
            }

            try
            {
                // try to authenticate
                // returns an anonymous principal if no credential was 
                var principal = _authN.Authenticate(request);

                if (principal == null)
                {
                    Tracing.Error(Area.HttpAuthentication, "Authentication returned null principal.");
                    throw new InvalidOperationException("No principal set");
                }

                if (principal.Identity.IsAuthenticated)
                {
                    Tracing.Information(Area.HttpAuthentication, "Authentication successful.");

                    // check for token request - if yes send token back and return
                    if (_authN.IsSessionTokenRequest(request))
                    {
                        Tracing.Information(Area.HttpAuthentication, "Request for session token.");
                        return SendSessionTokenResponse(principal, request);
                    }

                    // else set the principal
                    SetPrincipal(principal);
                }
            }
            catch (SecurityTokenValidationException ex)
            {
                Tracing.Error(Area.HttpAuthentication, "Error validating the token: " + ex.ToString());
                return SendUnauthorizedResponse(request);
            }
            catch (SecurityTokenException ex)
            {
                Tracing.Error(Area.HttpAuthentication, "Error validating the token: " + ex.ToString());
                return SendUnauthorizedResponse(request);
            }
            catch (Exception ex)
            {
                Tracing.Error(Area.HttpAuthentication, "Exception while validating the token: " + ex.ToString());
                return SendUnauthorizedResponse(request);
            }

            return base.SendAsync(request, cancellationToken).ContinueWith(
                (task) =>
                {
                    var response = task.Result;

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        SetAuthenticateHeader(response);
                    }

                    return response;
                });
        }

        private Task<HttpResponseMessage> SendUnauthorizedResponse(HttpRequestMessage request)
        {
            var unauthorizedResponse = request.CreateResponse(HttpStatusCode.Unauthorized);

            SetAuthenticateHeader(unauthorizedResponse);
            unauthorizedResponse.ReasonPhrase = "Unauthorized.";

            return Task.FromResult<HttpResponseMessage>(unauthorizedResponse);
        }

        private Task<HttpResponseMessage> SendSessionTokenResponse(ClaimsPrincipal principal, HttpRequestMessage request)
        {
            var token = _authN.CreateSessionToken(principal);
            var tokenResponse = _authN.CreateSessionTokenResponse(token);

            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(tokenResponse, Encoding.UTF8, "application/json");

            return Task.FromResult<HttpResponseMessage>(response);

            //return Task<HttpResponseMessage>.Factory.StartNew(() =>
            //{
            //    var response = new HttpResponseMessage(HttpStatusCode.OK);
            //    response.Content = new StringContent(tokenResponse, Encoding.UTF8, "application/json");

            //    return response;
            //});
        }

        protected virtual void SetAuthenticateHeader(HttpResponseMessage response)
        {
            if (_authN.Configuration.SendWwwAuthenticateResponseHeader)
            {
                Tracing.Verbose(Area.HttpAuthentication, "Setting Www-Authenticate header with scheme: " + _authN.Configuration.DefaultAuthenticationScheme);

                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(_authN.Configuration.DefaultAuthenticationScheme));
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

                Tracing.Information(Area.HttpAuthentication, "Authentication successful for: " + name);
            }
            else
            {
                Tracing.Information(Area.HttpAuthentication, "Setting anonymous principal.");
            }

            Thread.CurrentPrincipal = principal;

            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }
    }
}