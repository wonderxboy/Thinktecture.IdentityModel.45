using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityModel.Http.Hawk.Core.MessageContracts
{
    /// <summary>
    /// Represents an HTTP Request message applicable to Hawk authentication.
    /// </summary>
    public interface IRequestMessage : IMessage
    {
        /// <summary>
        /// Per-request placeholder for the challenge parameter
        /// </summary>
        string ChallengeParameter { get; set; }

        /// <summary>
        /// Host header value
        /// </summary>
        string Host { get; }

        /// <summary>
        /// X-Forwarded-For header value
        /// </summary>
        string ForwardedFor { get; }

        /// <summary>
        /// Authorization header value
        /// </summary>
        AuthenticationHeaderValue Authorization { get; set; }

        /// <summary>
        /// Request URI
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Query string setter for setting query string sans bewit.
        /// </summary>
        string QueryString { set; }

        /// <summary>
        /// HTTP Method
        /// </summary>
        HttpMethod Method { get; }
    }
}
