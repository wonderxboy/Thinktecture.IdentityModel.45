using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core.Extensions;

namespace Thinktecture.IdentityModel.Http.Hawk.Core
{
    /// <summary>
    /// Represents the HTTP authorization header in hawk scheme.
    /// </summary>
    internal class HawkSchemeHeader
    {
        /// <summary>
        /// Returns an AuthenticationResult object corresponding to the result of authentication done
        /// using the client supplied artifacts in the HTTP authorization header in hawk scheme.
        /// </summary>
        /// <param name="now">Current UNIX time in milliseconds.</param>
        /// <param name="request">Request object.</param>
        /// <param name="callback">The callback function that returns a Credential object corresponding to the identifier passed in.</param>
        /// <returns></returns>
        internal static async Task<AuthenticationResult> AuthenticateAsync(ulong now, HttpRequestMessage request,
                                            Func<string, Credential> callback)
        {
            ArtifactsContainer artifacts = null;
            Credential credential = null;

            if(request.HasValidHawkScheme())
            {
                if (ArtifactsContainer.TryParse(request.Headers.Authorization.Parameter, out artifacts))
                {
                    if (artifacts != null && artifacts.AreClientArtifactsValid)
                    {
                        credential = callback(artifacts.Id);
                        if (credential != null && credential.IsValid)
                        {
                            var normalizedRequest = new NormalizedRequest(request, artifacts);
                            var crypto = new Cryptographer(normalizedRequest, artifacts, credential);

                            if (await crypto.IsSignatureValidAsync(request.Content)) // MAC and hash checks
                            {
                                if (IsTimestampFresh(now, artifacts))
                                {
                                    // If you get this far, you are authentic. Welcome and thanks for flying Hawk!
                                    return new AuthenticationResult()
                                    {
                                        IsAuthentic = true,
                                        Artifacts = artifacts,
                                        Credential = credential,
                                        ApplicationSpecificData = artifacts.ApplicationSpecificData
                                    };
                                }
                                else
                                {
                                    // Authentic but for the timestamp freshness.
                                    // Give a chance to the client to correct the clocks skew.
                                    var timestamp = new NormalizedTimestamp(DateTime.UtcNow, credential);
                                    request.PutChallengeParameter(timestamp.ToWwwAuthenticateHeaderParameter());
                                }
                            }
                        }
                    }
                }
            }

            return new AuthenticationResult() { IsAuthentic = false };
        }

        /// <summary>
        /// Returns true if the timestamp sent in by the client is fresh subject to the 
        /// maximum allowed skew and the adjustment offset.
        /// </summary>
        private static bool IsTimestampFresh(ulong now, ArtifactsContainer artifacts)
        {
            now = now + UInt64.Parse(ConfigurationManager.AppSettings["LocalTimeOffsetMillis"]);

            ulong shelfLife = (UInt64.Parse(ConfigurationManager.AppSettings["ClockSkewSeconds"]) * 1000);
            var age = Math.Abs((artifacts.Timestamp * 1000.0) - now);

            return (age <= shelfLife);
        }
    }
}
