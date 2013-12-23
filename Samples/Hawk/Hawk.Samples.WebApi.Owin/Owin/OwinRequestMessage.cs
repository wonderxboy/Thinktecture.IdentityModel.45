using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Http.Hawk.Core.MessageContracts;

namespace Thinktecture.IdentityModel.Http.Hawk.Owin
{
    public class OwinRequestMessage : OwinMessage, IRequestMessage
    {
        private const string PARAMETER_KEY = "HK_Challenge";
        private const string OwinRequestHeadersKey = "owin.RequestHeaders";

        private OwinRequest request;

        public OwinRequestMessage(OwinRequest request)
            : base(request.Body, request.GetHeader(HawkConstants.ContentTypeHeaderName))
        {
            this.request = request;
        }

        public string ChallengeParameter
        {
            get
            {
                return this.request.Get<string>(PARAMETER_KEY);
            }
            set
            {
                this.request.Set<string>(PARAMETER_KEY, value);
            }
        }

        public string Host
        {
            get
            {
                return this.request.Host;
            }
        }

        public string ForwardedFor
        {
            get
            {
                string xff = this.request.GetHeader(HawkConstants.XffHeaderName);

                if (!String.IsNullOrWhiteSpace(xff))
                    xff = xff.Split(',')[0].Trim();

                return xff;
            }
        }

        public AuthenticationHeaderValue Authorization
        {
            get
            {
                string authorization = this.request.GetHeader(HawkConstants.AuthorizationHeaderName);

                return authorization != null ?
                    AuthenticationHeaderValue.Parse(authorization) :
                        null;
            }
            set
            {
                this.Headers[HawkConstants.AuthorizationHeaderName] = new[] { value.ToString() };
            }
        }

        public Uri Uri
        {
            get
            {
                return this.request.Uri;
            }
        }

        public HttpMethod Method
        {
            get
            {
                return new HttpMethod(this.request.Method);
            }
        }

        public string ContentType
        {
            get
            {
                string contentType = this.request.GetHeader(HawkConstants.ContentTypeHeaderName);

                if (!String.IsNullOrWhiteSpace(contentType))
                {
                    var header = MediaTypeHeaderValue.Parse(contentType);
                    if(header!= null)
                        return header.MediaType;
                }

                return null;
            }
        }

        public IDictionary<string, string[]> Headers
        {
            get
            {
                return this.request.Environment[OwinRequestHeadersKey] as IDictionary<string, string[]>;
            }
        }

        public string QueryString
        {
            set
            {
                this.request.QueryString = value ?? String.Empty;
            }
        }
    }
}
