using System.Net.Http;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;

namespace Thinktecture.IdentityModel.Http.Hawk.Core
{
    /// <summary>
    /// The class responsible for validating the HMAC in the request (in case of service) and the response (in case of the client)
    /// and signing the request (in case of client) and response (in case of service) by creating an HMAC.
    /// </summary>
    internal class Cryptographer
    {
        private readonly ArtifactsContainer artifacts = null;
        private readonly Credential credential = null;
        private readonly NormalizedRequest normalizedRequest = null;
        private readonly Hasher hasher = null;

        internal Cryptographer(NormalizedRequest request, ArtifactsContainer artifacts, Credential credential)
        {
            this.normalizedRequest = request;
            this.artifacts = artifacts;
            this.credential = credential;
            this.hasher = new Hasher(credential.Algorithm);
        }

        /// <summary>
        /// Returns true, if the HMAC computed for the normalized request matches the HMAC 
        /// sent in by the client (ArtifactsContainer.Mac) and if a payload hash is present,
        /// it matches the hash computed for the normalized payload as well.
        /// </summary>
        internal async Task<bool> IsSignatureValidAsync(HttpContent content)
        {
            return this.IsMacValid() && (this.IsHashNotPresent() || await this.IsHashValidAsync(content));
        }

        /// <summary>
        /// Creates the payload hash and the corresponding HMAC, and updates the artifacts object with these new values.
        /// </summary>
        internal async Task SignAsync(HttpContent newContent)
        {
            byte[] responsePayloadHash = null;
            if (newContent != null)
            {
                var payload = new NormalizedPayload(newContent);
                byte[] data = await payload.ToBytesAsync();

                responsePayloadHash = hasher.ComputeHash(data);
            }

            artifacts.PayloadHash = responsePayloadHash;
            artifacts.Mac = hasher.ComputeHmac(this.normalizedRequest.ToBytes(), credential.Key);
        }

        private bool IsMacValid()
        {
            byte[] data = this.normalizedRequest.ToBytes();
            // data, at this point has the hash coming in over the wire and hence mac computed is based 
            // on the hash over the wire and not over the computed hash

            return this.hasher.IsValidMac(data, credential.Key, artifacts.Mac);
        }

        private bool IsHashNotPresent()
        {
            return artifacts.PayloadHash == null || artifacts.PayloadHash.Length == 0;
        }

        private async Task<bool> IsHashValidAsync(HttpContent payload)
        {
            var normalizedPayload = new NormalizedPayload(payload);
            byte[] data = await normalizedPayload.ToBytesAsync();

            return this.hasher.IsValidHash(data, artifacts.PayloadHash);
        }
    }
}
