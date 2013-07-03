using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;

namespace Thinktecture.IdentityModel.Http.Hawk.WebApi
{
    /// <summary>
    /// The message handler that performs the authentication based on the authenticity of the HMAC.
    /// Add a new instance of this handler to config.MessageHandlers in WebApiConfig.Register().
    /// </summary>
    internal class HawkAuthenticationHandler : DelegatingHandler
    {
        private readonly Func<string, Credential> credentialsCallback = null;
        private readonly Func<HttpResponseMessage, string> normalizationCallback = null;
        private readonly Func<HttpRequestMessage, string, bool> verificationCallback = null;

        /// <summary>
        /// The message handler that authenticates the request using Hawk.
        /// </summary>
        /// <param name="credentialsCallback">The callback function that returns a Credential object corresponding to the identifier passed in.</param>
        /// <param name="normalizationCallback">The callback function that returns the application specific data that the web api can send in the response.</param>
        /// <param name="verificationCallback">The callback function that returns true, if the application specific data in the request is valid.</param>
        internal HawkAuthenticationHandler(Func<string, Credential> credentialsCallback,
                                                Func<HttpResponseMessage, string> normalizationCallback = null,
                                                    Func<HttpRequestMessage, string, bool> verificationCallback = null)
        {
            if (credentialsCallback == null)
                throw new ArgumentNullException("Credentials callback is null");

            this.credentialsCallback = credentialsCallback;
            this.normalizationCallback = normalizationCallback;
            this.verificationCallback = verificationCallback;
        }

        protected async override Task<HttpResponseMessage> SendAsync(
                                        HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                HawkServer server = new HawkServer(request, credentialsCallback, verificationCallback);

                var principal = await server.AuthenticateAsync();

                if (principal != null)
                {
                    Thread.CurrentPrincipal = principal;

                    if (HttpContext.Current != null)
                        HttpContext.Current.User = principal;
                }

                var response = await base.SendAsync(request, cancellationToken);

                await server.CreateServerAuthorizationAsync(response, this.normalizationCallback);

                return response;
            }
            catch (Exception)
            {
                var response = request.CreateResponse(HttpStatusCode.Unauthorized);
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(HawkConstants.Scheme));

                return response;
            }
        }
    }
}
