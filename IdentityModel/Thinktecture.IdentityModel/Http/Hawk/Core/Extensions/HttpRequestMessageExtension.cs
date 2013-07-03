using System;
using System.Net.Http;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;

namespace Thinktecture.IdentityModel.Http.Hawk.Core.Extensions
{
    internal static class HttpRequestMessageExtension
    {
        private const string PARAMETER_KEY = "HK_Challenge";

        /// <summary>
        /// Stores the parameter to be used with the WWW-Authenticate header in the request object.
        /// </summary>
        internal static void PutChallengeParameter(this HttpRequestMessage request, string parameter)
        {
            request.Properties[PARAMETER_KEY] = parameter;
        }

        /// <summary>
        /// Retrieves the parameter to be used with the WWW-Authenticate header from the request object.
        /// </summary>
        internal static string GetChallengeParameter(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(PARAMETER_KEY))
            {
                return (string)request.Properties[PARAMETER_KEY];
            }

            return null;
        }

        /// <summary>
        /// Returns true if there is an Authorization HTTP header present in the request, the header has a scheme 
        /// that is "hawk" and that the parameter in the header is not empty.
        /// </summary>
        internal static bool HasValidHawkScheme(this HttpRequestMessage request)
        {
            var header = request.Headers.Authorization;

            return (header != null && header.Scheme.ToLower() == HawkConstants.Scheme &&
                                    !String.IsNullOrWhiteSpace(header.Parameter));
        }
    }
}
