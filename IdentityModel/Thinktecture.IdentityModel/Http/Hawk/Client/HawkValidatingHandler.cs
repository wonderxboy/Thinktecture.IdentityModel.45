using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Client;

namespace Thinktecture.IdentityModel.Http.Hawk.Client
{
    /// <summary>
    /// The client side message handler that adds the Authorization header to the request and validates the response.
    /// </summary>
    public class HawkValidationHandler : DelegatingHandler
    {
        private readonly Func<Credential> credentialsCallback = null;
        private readonly Func<HttpRequestMessage, string> normalizationCallback = null;
        private readonly Func<HttpResponseMessage, string, bool> verificationCallback = null;

        /// <summary>
        /// The client side message handler that adds the Authorization header to the request and validates the response.
        /// </summary>
        /// <param name="credentialsCallback">The callback function that returns the Credential object corresponding to the user.</param>
        /// <param name="normalizationCallback">The callback function that returns the application specific data that the client can send in the request.</param>
        /// <param name="verificationCallback">The callback function that returns true, if the application specific data in the response is valid.</param>
        public HawkValidationHandler(Func<Credential> credentialsCallback,
                                        Func<HttpRequestMessage, string> normalizationCallback = null,
                                            Func<HttpResponseMessage, string, bool> verificationCallback = null)
        {
            if (credentialsCallback == null)
                throw new ArgumentNullException("Credentials callback is null");

            this.credentialsCallback = credentialsCallback;
            this.normalizationCallback = normalizationCallback;
            this.verificationCallback = verificationCallback;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var client = new HawkClient(this.credentialsCallback);

            if (this.normalizationCallback != null)
                client.ApplicationSpecificData = this.normalizationCallback(request);

            await client.CreateClientAuthorizationAsync(request);

            var response = await base.SendAsync(request, cancellationToken);

            if (!await client.AuthenticateAsync(response))
                throw new SecurityException("Invalid Mac and/or hash. Response possibly tampered.");

            bool isValidAppSpecificData = this.verificationCallback == null ||
                                                    this.verificationCallback(response, client.WebApiSpecificData);
            if (!isValidAppSpecificData)
                throw new SecurityException("Invalid Application Specific Data");

            return response;
        }
    }
}