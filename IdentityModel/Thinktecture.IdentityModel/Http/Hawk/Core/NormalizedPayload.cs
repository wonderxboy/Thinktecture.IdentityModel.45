using System;
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

        private readonly string body = null;
        private readonly string contentType = null;

        internal NormalizedPayload(string body, string contentType)
        {
            this.body = body;
            this.contentType = contentType;
        }

        /// <summary>
        /// Returns the normalized payload bytes.
        /// </summary>
        internal byte[] ToBytes()
        {
            if (this.body != null)
            {
                StringBuilder builder = new StringBuilder();

                builder
                    .AppendNewLine(PREAMBLE)
                    .AppendNewLine(contentType == null ? String.Empty : contentType.ToLower())
                    .AppendNewLine(this.body);

                return builder.ToString().ToBytesFromUtf8();
            }

            return null;
        }
    }
}
