using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;

namespace Thinktecture.IdentityModel.Http.Hawk.Owin.Extensions
{
    internal static class OwinRequestExtension
    {
        internal static bool IsPayloadHashPresent(this OwinRequest request)
        {
            string authorization = request.GetHeader(HawkConstants.AuthorizationHeaderName);

            if (!String.IsNullOrWhiteSpace(authorization))
            {
                // TODO - Replace the following block with
                // return ArtifactsContainer.IsPayloadHashPresent(parameter);
                // once OWIN is part of tt.idm
                string parameter = AuthenticationHeaderValue.Parse(authorization).Parameter;
                string pattern = String.Format(@"({0})=""([^""\\]*)""\s*(?:,\s*|$)", "hash");

                if (!String.IsNullOrWhiteSpace(parameter))
                    if (System.Text.RegularExpressions.Regex.IsMatch(parameter, pattern))
                        return true;
            }

            return false;
        }
    }
}
