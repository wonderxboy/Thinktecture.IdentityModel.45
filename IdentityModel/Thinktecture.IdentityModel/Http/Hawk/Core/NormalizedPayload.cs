using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core.Extensions;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;

namespace Thinktecture.IdentityModel.Http.Hawk.Core
{
    /// <summary>
    /// Represents the normalized payload, in the following format.
    /// hawk.1.payload\n
    /// content type\n
    /// body content\n
    /// </summary>
    internal class NormalizedPayload
    {
        private const string PREAMBLE = HawkConstants.Scheme + "." + HawkConstants.Version + ".payload"; // hawk.1.payload

        private readonly HttpContent content = null;

        internal NormalizedPayload(HttpContent content)
        {
            this.content = content;
        }

        /// <summary>
        /// Returns the normalized payload bytes.
        /// </summary>
        internal async Task<byte[]> ToBytesAsync()
        {
            if (this.content != null)
            {
                string contentType = String.Empty;
                if (content.Headers.ContentType != null)
                {
                    contentType = content.Headers.ContentType.MediaType.ToLower();
                }

                string body = await content.ReadAsStringAsync();

                StringBuilder builder = new StringBuilder();

                builder
                    .AppendNewLine(PREAMBLE)
                    .AppendNewLine(contentType)
                    .AppendNewLine(body ?? String.Empty);

                return builder.ToString().ToBytesFromUtf8();
            }

            return null;
        }
    }
}
