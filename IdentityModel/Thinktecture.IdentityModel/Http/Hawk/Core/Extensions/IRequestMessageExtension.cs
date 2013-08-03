using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Http.Hawk.Core.MessageContracts;

namespace Thinktecture.IdentityModel.Http.Hawk.Core.Extensions
{
    internal static class IRequestMessageExtension
    {
        /// <summary>
        /// Returns true if there is an Authorization HTTP header present in the request, the header has a scheme 
        /// that is "hawk" and that the parameter in the header is not empty.
        /// </summary>
        internal static bool HasValidHawkScheme(this IRequestMessage request)
        {
            var header = request.Authorization;

            return (header != null && header.Scheme.ToLower() == HawkConstants.Scheme &&
                                    !String.IsNullOrWhiteSpace(header.Parameter));
        }
    }
}
