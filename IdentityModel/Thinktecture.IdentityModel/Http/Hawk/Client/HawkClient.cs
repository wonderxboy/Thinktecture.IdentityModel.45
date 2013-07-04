using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core.Extensions;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;

namespace Thinktecture.IdentityModel.Http.Hawk.Core.Client
{
    /// <summary>
    /// The counterpart of HawkServer in the client side that creates the HTTP Authorization header in hawk scheme
    /// and authenticates the server response by validating the Server-Authorization response header.
    /// HawkClient is for per-request use.
    /// </summary>
    public class HawkClient
    {
        private static readonly object myPrecious = new object();

        private readonly Func<Credential> credentialFunc = null;
        private readonly bool enableResponseValidation;
        private readonly bool enableAutoCompensationForClockSkew;

        private ArtifactsContainer artifacts = null;
        private Cryptographer crypto = null;

        /// <summary>
        /// Authenticates the server response by reading the Server-Authorization header and creates 
        /// the the HTTP Authorization header in hawk scheme.
        /// </summary>
        /// <param name="credentialFunc">A callback to receive the Hawk credential</param>
        public HawkClient(Func<Credential> credentialFunc)
        {
            if (credentialFunc == null)
                throw new ArgumentNullException("Credential callback is null");

            string enableValidation = ConfigurationManager.AppSettings["EnableResponseValidation"];
            string enableAutoCompensation = ConfigurationManager.AppSettings["EnableAutoCompensationForClockSkew"];

            if (enableValidation == null || enableAutoCompensation == null)
            {
                string message = "Required config settings of EnableResponseValidation or EnableAutoCompensationForClockSkew missing.";
                Tracing.Error(message);

                throw new Exception(message);
            }

            this.credentialFunc = credentialFunc;
            this.enableResponseValidation = enableValidation.ToBool();
            this.enableAutoCompensationForClockSkew = enableAutoCompensation.ToBool();
        }

        /// <summary>
        /// Added to current date time before computing the UNIX time. HawkClient can automatically
        /// adjust the value in this property based on the timestamp sent by the service in the 
        /// WWW-Authenticate header in an attempt to keep the server and the client clocks in sync.
        /// </summary>
        public static int CompensatorySeconds { get; private set; }

        /// <summary>
        /// Application specific data that the client sends along in the request.
        /// </summary>
        public string ApplicationSpecificData { get; set; }

        /// <summary>
        /// Application specific data that the web API has sent along in the response.
        /// </summary>
        public string WebApiSpecificData { get; private set; }

        /// <summary>
        /// Returns true, if the HMAC computed for the response payload matches the HMAC in the
        /// Server-Authorization response header. This method also sets the compensation field so 
        /// that the timestamp in the subsequent requests are adjusted to reduce the clock skew.
        /// </summary>
        public async Task<bool> AuthenticateAsync(HttpResponseMessage response)
        {
            if (response.StatusCode != HttpStatusCode.Unauthorized &&
                    this.enableResponseValidation &&
                        await this.IsResponseTamperedAsync(artifacts, crypto, response))
                return false;

            if (this.enableAutoCompensationForClockSkew &&
                    this.IsTimestampResponseTampered(artifacts, response))
                return false;

            return true;
        }

        /// <summary>
        /// Creates the HTTP Authorization header in hawk scheme.
        /// The counterpart of the CreateServerAuthorization method in HawkServer.
        /// </summary>
        public async Task CreateClientAuthorizationAsync(HttpRequestMessage request)
        {
            await CreateClientAuthorizationInternalAsync(request, DateTime.UtcNow);
        }

        /// <summary>
        /// Adds the bewit to the query string of the specified HttpRequestMessage object and
        /// returns the bewit string.
        /// </summary>
        public async Task<string> CreateBewitAsync(HttpRequestMessage request, int lifeSeconds)
        {
            return await CreateBewitInternalAsync(request, DateTime.UtcNow, lifeSeconds);
        }

        /// <summary>
        /// Adds the bewit to the query string of the specified HttpRequestMessage object and 
        /// returns the bewit string.
        /// </summary>
        internal async Task<string> CreateBewitInternalAsync(HttpRequestMessage request, DateTime utcNow, int lifeSeconds)
        {
            var bewit = new Bewit(request, credentialFunc(), utcNow, lifeSeconds, this.ApplicationSpecificData);
            string bewitString = await bewit.ToBewitStringAsync();

            string parameter = String.Format("{0}={1}", HawkConstants.Bewit, bewitString);

            string queryString = request.RequestUri.Query;
            queryString = String.IsNullOrWhiteSpace(queryString) ? parameter : queryString.Substring(1) + "&" + parameter;

            var builder = new UriBuilder(request.RequestUri);
            builder.Query = queryString;

            request.RequestUri = builder.Uri;

            return bewitString;
        }

        /// <summary>
        /// Creates the HTTP Authorization header in hawk scheme.
        /// </summary>
        internal async Task CreateClientAuthorizationInternalAsync(HttpRequestMessage request, DateTime utcNow)
        {
            var credential = credentialFunc();
            this.artifacts = new ArtifactsContainer()
            {
                Id = credential.Id,
                Timestamp = utcNow.AddSeconds(HawkClient.CompensatorySeconds).ToUnixTime(),
                Nonce = NonceGenerator.Generate()
            };

            if (!String.IsNullOrWhiteSpace(this.ApplicationSpecificData))
                this.artifacts.ApplicationSpecificData = this.ApplicationSpecificData;

            var normalizedRequest = new NormalizedRequest(request, this.artifacts);
            this.crypto = new Cryptographer(normalizedRequest, this.artifacts, credential);

            // Sign the request
            await crypto.SignAsync(request.Content);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                                                HawkConstants.Scheme,
                                                this.artifacts.ToAuthorizationHeaderParameter());
        }

        /// <summary>
        /// Returns true if the server response HMAC cannot be validated, indicating possible tampering.
        /// </summary>
        private async Task<bool> IsResponseTamperedAsync(ArtifactsContainer artifacts, Cryptographer crypto,
                                                        HttpResponseMessage response)
        {
            if (response.Headers.Contains(HawkConstants.ServerAuthorizationHeaderName))
            {
                string header = response.Headers.GetValues(HawkConstants.ServerAuthorizationHeaderName).FirstOrDefault();

                if (!String.IsNullOrWhiteSpace(header) &&
                                    header.Substring(0, HawkConstants.Scheme.Length).ToLower() == HawkConstants.Scheme)
                {
                    ArtifactsContainer serverAuthorizationArtifacts;
                    if (ArtifactsContainer.TryParse(header.Substring(HawkConstants.Scheme.Length + " ".Length),
                                                                                    out serverAuthorizationArtifacts))
                    {
                        // To validate response, ext, hash, and mac in the request artifacts must be
                        // replaced with the ones from the server.
                        artifacts.ApplicationSpecificData = serverAuthorizationArtifacts.ApplicationSpecificData;
                        artifacts.PayloadHash = serverAuthorizationArtifacts.PayloadHash;
                        artifacts.Mac = serverAuthorizationArtifacts.Mac;

                        bool isValid = await crypto.IsSignatureValidAsync(response.Content);

                        if (isValid)
                            this.WebApiSpecificData = serverAuthorizationArtifacts.ApplicationSpecificData;

                        return !isValid;
                    }
                }
            }

            return true; // Missing header means possible tampered response (to err on the side of caution).
        }

        /// <summary>
        /// Returns true, if there is a WWW-Authenticate header containing ts and tsm but mac
        /// computed for ts does not match tsm, indicating possible tampering. Otherwise, returns false.
        /// This method also sets the compensation field so that the timestamp in the subsequent requests
        /// are adjusted to reduce the clock skew.
        /// </summary>
        private bool IsTimestampResponseTampered(ArtifactsContainer artifacts, HttpResponseMessage response)
        {
            if (response.Headers.WwwAuthenticate != null)
            {
                var wwwHeader = response.Headers.WwwAuthenticate.FirstOrDefault();

                if (wwwHeader != null && wwwHeader.Scheme.ToLower() == HawkConstants.Scheme)
                {
                    string parameter = wwwHeader.Parameter;

                    ArtifactsContainer timestampArtifacts;
                    if (!String.IsNullOrWhiteSpace(parameter) &&
                                    ArtifactsContainer.TryParse(parameter, out timestampArtifacts))
                    {
                        var ts = new NormalizedTimestamp(timestampArtifacts.Timestamp, credentialFunc());

                        if (!ts.IsValid(timestampArtifacts.TimestampMac))
                            return true;

                        lock (myPrecious)
                            HawkClient.CompensatorySeconds = (int)(timestampArtifacts.Timestamp - DateTime.UtcNow.ToUnixTime());

                        Tracing.Information("HawkClient.CompensatorySeconds set to " + HawkClient.CompensatorySeconds);
                    }
                }
            }

            return false;
        }
    }
}