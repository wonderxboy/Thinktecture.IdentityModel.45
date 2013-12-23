using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Http.Hawk.Core.MessageContracts;

namespace Thinktecture.IdentityModel.Http.Hawk.WebApi
{
    public class WebApiResponseMessage : WebApiMessage, IResponseMessage
    {
        private readonly HttpResponseMessage response = null;

        public WebApiResponseMessage(HttpResponseMessage response) : base(response.Content)
        {
            this.response = response;

            this.response.Headers.ToList()
                .ForEach(h => this.messageHeaders.Add(h.Key, h.Value.ToArray()));
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return this.response.StatusCode;
            }
        }

        public AuthenticationHeaderValue WwwAuthenticate
        {
            get
            {
                var headers = this.response.Headers.WwwAuthenticate;

                if (headers != null)
                    return headers.FirstOrDefault(h => h.Scheme.ToLower() == HawkConstants.Scheme);

                return null;
            }
        }

        public void AddHeader(string name, string value)
        {
            this.response.Headers.Add(name, value);
        }
    }
}
