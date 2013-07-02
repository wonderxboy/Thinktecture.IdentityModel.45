using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.IdentityModel.Tokens.Http
{
    /// <summary>
    /// A DelegatingHandler that AuthenticationHandler can instantiate and plug into the
    /// ASP.NET Web API pipeline on-the-fly.
    /// </summary>
    public abstract class PluggableDelegatingHandler : DelegatingHandler
    {
        /// <summary>
        /// Visibility increasing method for AuthenticationHandler to delegate authentication on-the-fly to
        /// a pluggable delegating handler. AuthenticationHandler calls and returns the resulting
        /// Task object to the pipeline instead of running its own authentication code in the pipeline.
        /// </summary>
        public Task<HttpResponseMessage> DelegatingSendAsync(
                                        HttpRequestMessage request,
                                            CancellationToken cancellationToken)
        {
            return this.SendAsync(request, cancellationToken);
        }
    }
}
