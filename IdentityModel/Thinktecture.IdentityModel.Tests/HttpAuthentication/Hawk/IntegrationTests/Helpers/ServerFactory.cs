using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;

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

        internal static HttpServer Create(Func<HttpResponseMessage, string> normalizationCallback = null,
                                            Func<HttpRequestMessage, string, bool> verificationCallback = null)
        {
            var configuration = new HttpConfiguration();

            configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            Func<string, Credential> callback = (id) => Credentials.FirstOrDefault(c => c.Id == id);

            //var authConfig = new AuthenticationConfiguration() { RequireSsl = false };
            //authConfig.AddHawkAuthentication(callback, allowBewit: true,
            //                                            normalizationCallback: normalizationCallback,
            //                                                verificationCallback: verificationCallback);

            //configuration.MessageHandlers.Add(new AuthenticationHandler(authConfig));

            return new HttpServer(configuration);
        }
    }
}
