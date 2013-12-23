﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Http.Hawk.Core.MessageContracts;

namespace Thinktecture.IdentityModel.Http.Hawk.WebApi
{
    public class WebApiRequestMessage : WebApiMessage, IRequestMessage
    {
        private const string PARAMETER_KEY = "HK_Challenge";

        private readonly HttpRequestMessage request = null;

        public WebApiRequestMessage(HttpRequestMessage request) : base(request.Content)
        {
            this.request = request;

            this.request.Headers.ToList()
                .ForEach(h => this.messageHeaders.Add(h.Key, h.Value.ToArray()));
        }

        public string ChallengeParameter
        {
            get
            {
                if (this.request.Properties.ContainsKey(PARAMETER_KEY))
                {
                    return (string)this.request.Properties[PARAMETER_KEY];
                }

                return null;
            }
            set
            {
                this.request.Properties[PARAMETER_KEY] = value;
            }
        }

        public string Host
        {
            get
            {
                return this.request.Headers.Host;
            }
        }

        public string ForwardedFor
        {
            get
            {
                string xff = String.Empty;

                if (this.request.Headers.Contains(HawkConstants.XffHeaderName))
                    xff = this.request.Headers.GetValues(HawkConstants.XffHeaderName).FirstOrDefault();

                if (!String.IsNullOrWhiteSpace(xff))
                    xff = xff.Split(',')[0].Trim();

                return xff;
            }
        }

        public AuthenticationHeaderValue Authorization
        {
            get
            {
                return this.request.Headers.Authorization;
            }
            set
            {
                this.request.Headers.Authorization = value;
            }
        }

        public Uri Uri
        {
            get
            {
                return this.request.RequestUri;
            }
        }

        public HttpMethod Method
        {
            get
            {
                return this.request.Method;
            }
        }

        public string QueryString
        {
            set
            {
                UriBuilder builder = new UriBuilder(this.request.RequestUri);
                builder.Query = value ?? String.Empty;

                this.request.RequestUri = builder.Uri;
            }
        }
    }
}
