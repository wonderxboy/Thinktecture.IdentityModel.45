using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core.Extensions;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;

namespace Thinktecture.IdentityModel.Http.Hawk.Core
{
    /// <summary>
    /// Authenticates the incoming request based on the Authorize request header or bewit query string parameter
    /// and sets the Server-Authorization or the WWW-Authenticate response header. HawkServer is for per-request use.
    /// </summary>
    public class HawkServer
    {
        private readonly HttpRequestMessage request = null;
        private readonly Func<string, Credential> credentialsFunc = null;
        private readonly Func<HttpRequestMessage, string, bool> verificationCallback = null;

        private ulong now = 0;
        private AuthenticationResult result = null;
        private bool isBewitRequest = false;

        /// <summary>
        /// Authenticates the incoming request based on the Authorize request header or bewit query string parameter
        /// </summary>
        /// <param name="request">The request object to be authenticated</param>
        /// <param name="credentialsFunc">The callback function that returns a Credential object corresponding to the identifier passed in.</param>
        /// <param name="applicationSpecificDataVerificationCallback">The callback function that returns true, if the application specific data in the request is valid.</param>
        public HawkServer(HttpRequestMessage request,
                            Func<string, Credential> credentialsFunc,
                                Func<HttpRequestMessage, string, bool> applicationSpecificDataVerificationCallback = null)
        {
            now = DateTime.UtcNow.ToUnixTimeMillis(); // Record time before doing anything else

            if (credentialsFunc == null)
                throw new ArgumentNullException("Invalid credentials callback");

            this.request = request;
            this.credentialsFunc = credentialsFunc;
            this.verificationCallback = applicationSpecificDataVerificationCallback;
        }

        /// <summary>
        /// Returns a ClaimsPrincipal object with the NameIdentifier and Name claims, if the request can be
        /// successfully authenticated based on query string parameter bewit or HTTP Authorization header (hawk scheme).
        /// </summary>
        public async Task<ClaimsPrincipal> AuthenticateAsync()
        {
            string bewit;
            bool isBewit = Bewit.TryGetBewit(this.request, out bewit);

            var authentication = isBewit ?
                                        Bewit.AuthenticateAsync(bewit, now, request, credentialsFunc) :
                                            HawkSchemeHeader.AuthenticateAsync(now, request, credentialsFunc);

            this.result = await authentication;

            if (result.IsAuthentic)
            {
                // At this point, authentication is successful but make sure the request parts match what is in the
                // application specific data 'ext' parameter by invoking the callback passing in the request object and 'ext'.
                // The application specific data is considered verified, if the callback is not set or it returns true.
                bool isAppSpecificDataVerified = this.verificationCallback == null ||
                                                    this.verificationCallback(request, result.ApplicationSpecificData);
                
                if (isAppSpecificDataVerified)
                {
                    // Set the flag so that Server-Authorization header is not sent for bewit requests.
                    this.isBewitRequest = isBewit;

                    var idClaim = new Claim(ClaimTypes.NameIdentifier, result.Credential.Id);
                    var nameClaim = new Claim(ClaimTypes.Name, result.Credential.User);
                    
                    var identity = new ClaimsIdentity(new[] { idClaim, nameClaim }, HawkConstants.Scheme);

                    return new ClaimsPrincipal(identity);
                }
            }
            
            return null;
        }

        /// <summary>
        /// Adds the WWW-Authenticate header in case of a 401 - Unauthorized response and
        /// the Server-Authorization header in case of a successful request.
        /// </summary>
        public async Task CreateServerAuthorizationAsync(HttpResponseMessage response, Func<HttpResponseMessage, string> normalizationCallback)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var header = new AuthenticationHeaderValue(HawkConstants.Scheme, request.GetChallengeParameter());
                response.Headers.WwwAuthenticate.Add(header);
            }
            else
            {
                if (this.result != null && this.result.IsAuthentic && !this.isBewitRequest) // No Server-Authorization header for bewit requests
                {
                    if (normalizationCallback != null)
                        this.result.Artifacts.ApplicationSpecificData = normalizationCallback(response);

                    // Sign the response
                    var normalizedRequest = new NormalizedRequest(request, this.result.Artifacts);
                    var crypto = new Cryptographer(normalizedRequest, this.result.Artifacts, this.result.Credential);
                    await crypto.SignAsync(response.Content);

                    string authorization = this.result.Artifacts.ToServerAuthorizationHeaderParameter();

                    if (!String.IsNullOrWhiteSpace(authorization))
                        response.Headers.Add(HawkConstants.ServerAuthorizationHeaderName,
                                                HawkConstants.Scheme + " " + authorization);
                }
            }
        }
    }
}
