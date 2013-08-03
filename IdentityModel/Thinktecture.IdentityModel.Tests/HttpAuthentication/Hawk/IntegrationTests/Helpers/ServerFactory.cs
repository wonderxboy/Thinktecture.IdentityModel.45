using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Http.Hawk.Core.MessageContracts;
using Thinktecture.IdentityModel.Http.Hawk.WebApi;

namespace Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests.Helpers
{
    class ServerFactory
    {
        internal static List<Credential> Credentials
        {
            get
            {
                return new List<Credential>()
                {
                    new Credential()
                    {
                        Id = "dh37fgj492je",
                        Algorithm = SupportedAlgorithms.SHA256,
                        User = "Steve",
                        Key = "werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn"
                    }
                };
            }
        }

        internal static Credential DefaultCredential
        {
            get
            {
                return Credentials.FirstOrDefault();
            }
        }

        internal static HttpServer Create(Func<IResponseMessage, string> normalizationCallback = null,
                                            Func<IRequestMessage, string, bool> verificationCallback = null)
        {
            var configuration = new HttpConfiguration();

            configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var options = new Options()
            {
                ClockSkewSeconds = 60,
                LocalTimeOffsetMillis = 0,
                CredentialsCallback = (id) => Credentials.FirstOrDefault(c => c.Id == id),
                NormalizationCallback = normalizationCallback,
                VerificationCallback = verificationCallback,
                ResponsePayloadHashabilityCallback = (req) => true
            };

            var hawkHandler = new HawkAuthenticationHandler(options);
            configuration.MessageHandlers.Add(hawkHandler);

            return new HttpServer(configuration);
        }
    }
}
