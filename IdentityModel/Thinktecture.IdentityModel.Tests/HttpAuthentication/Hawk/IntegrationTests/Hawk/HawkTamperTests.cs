using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel.Http.Hawk.Client;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Http.Hawk.Core.MessageContracts;
using Thinktecture.IdentityModel.Http.Hawk.WebApi;
using Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests.Helpers;

namespace Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests
{
    [TestClass]
    public class HawkTamperTests
    {
        const string URI = "http://localhost/api/values";

        private HttpServer server = null;

        [TestInitialize()]
        public void Initialize()
        {
            server = ServerFactory.Create();

            // Clear the context
            Thread.CurrentPrincipal = new GenericPrincipal(
                                            new GenericIdentity(String.Empty),
                                                    null);
        }

        [TestMethod]
        public async Task MustReturn401WhenHawkParameterHasTamperedIdOrNonceOrTsOrMacOrHashOrExt()
        {
            await MustReturn401WhenHawkParameterIsTampered(tamperedAttribute: "id");
            await MustReturn401WhenHawkParameterIsTampered(tamperedAttribute: "nonce");
            await MustReturn401WhenHawkParameterIsTampered(tamperedAttribute: "ts");
            await MustReturn401WhenHawkParameterIsTampered(tamperedAttribute: "mac");
            await MustReturn401WhenHawkParameterIsTampered(tamperedAttribute: "hash");
            await MustReturn401WhenHawkParameterIsTampered(tamperedAttribute: "ext");
        }

        [TestMethod]
        public async Task MustReturn401WhenMacCreatedUsingBadSymmetricKey()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var badCredential = ServerFactory.Credentials.First();
                    badCredential.Key = "werxhqb98"; // Some key other than werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn

                    var client = ClientFactory.Create(() => badCredential);
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenBodyContentIsChangedAfterMacAndHashAreComputed()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, URI))
                {
                    request.Content = new ObjectContent<string>("Steve", new JsonMediaTypeFormatter());

                    var client = ClientFactory.Create(requestPayloadHashabilityCallback: (r) => true);
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request)); // Hash is now based on "Steve"

                    // Changing body to "Stephen"
                    request.Content = new ObjectContent<string>("Stephen", new JsonMediaTypeFormatter());

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenQueryStringIsChangedAfterMacIsComputed()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI + "?firstname=Louis&&lastname=Phillipe"))
                {
                    var client = ClientFactory.Create();
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request)); // Hash is now based on ?firstname=Louis&&lastname=Phillipe

                    // Changing query string to ?firstname=Luis&&lastname=Phillip
                    request.RequestUri = new Uri(URI + "?firstname=Luis&&lastname=Phillip");

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                    }
                }
            }
        }

        [TestMethod]
        public async Task TamperedResponseMacOrHashMustFailClientAuthentication()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var client = ClientFactory.Create();
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                        string header = response.Headers.GetValues(HawkConstants.ServerAuthorizationHeaderName).FirstOrDefault();
                        Assert.IsTrue(await client.AuthenticateAsync(new WebApiResponseMessage(response)));

                        string tamperedHeader = header.Replace("mac=\"", "mac=\"1234"); // mac="abc" => mac = "1234abc"
                        response.Headers.Remove(HawkConstants.ServerAuthorizationHeaderName);
                        response.Headers.Add(HawkConstants.ServerAuthorizationHeaderName, tamperedHeader);
                        Assert.IsFalse(await client.AuthenticateAsync(new WebApiResponseMessage(response)));

                        tamperedHeader = header.Replace("hash=\"", "hash=\"1234"); // hash="abc" => hash = "1234abc"
                        response.Headers.Remove(HawkConstants.ServerAuthorizationHeaderName);
                        response.Headers.Add(HawkConstants.ServerAuthorizationHeaderName, tamperedHeader);
                        Assert.IsFalse(await client.AuthenticateAsync(new WebApiResponseMessage(response)));
                    }
                }
            }
        }

        [TestMethod]
        public async Task TamperedTimestampMacMustFailClientAuthentication()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var client = ClientFactory.Create();

                    await client.CreateClientAuthorizationInternalAsync(new WebApiRequestMessage(request),
                                                                            DateTime.UtcNow.AddMinutes(-2));

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        var wwwheader = response.Headers.WwwAuthenticate.FirstOrDefault();
                        Assert.IsNotNull(wwwheader);
                        Assert.AreEqual("hawk", wwwheader.Scheme);

                        Assert.IsNotNull(wwwheader.Parameter);
                        string tsParameter = wwwheader.Parameter;

                        // ts and tsm must be present
                        Assert.IsTrue(ParameterChecker.IsFieldPresent(tsParameter, "ts"));
                        Assert.IsTrue(ParameterChecker.IsFieldPresent(tsParameter, "tsm"));
                        Assert.IsTrue(await client.AuthenticateAsync(new WebApiResponseMessage(response)));

                        string tamperedtsParameter = tsParameter.Replace("tsm=\"", "tsm=\"1234"); // tsm="abc" => tsm = "1234abc"
                        response.Headers.WwwAuthenticate.Remove(wwwheader);
                        response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("hawk", tamperedtsParameter));
                        Assert.IsFalse(await client.AuthenticateAsync(new WebApiResponseMessage(response)));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenProtectedCustomHeaderIsTampered()
        {
            Func<IRequestMessage, string, bool> verificationCallback = (request, appSpecificData) =>
            {
                // Client and server decided that appSpecificData will be
                // header name:header value, like so: X-Request-Header:Swoosh

                if (String.IsNullOrEmpty(appSpecificData))
                    return true; // Nothing to check against

                var parts = appSpecificData.Split(':');
                string headerName = parts[0];
                string value = parts[1];

                if (request.Headers.ContainsKey(headerName) &&
                    request.Headers[headerName].First().Equals(value))
                    return true;

                return false;
            };

            server = ServerFactory.Create(null, verificationCallback);

            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    // Sensitive header to protect but simulate tampering by changing the value
                    request.Headers.Add("X-Request-Header", "Tampered Swoosh");

                    var client = ClientFactory.Create(
                        normalizationCallback: (r) => "X-Request-Header:Swoosh"); // Put the sensitive header in the earlier decided format

                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                    }
                }
            }
        }

        private static async Task MustReturn401WhenHawkParameterIsTampered(string tamperedAttribute)
        {
            using (var invoker = new HttpMessageInvoker(ServerFactory.Create()))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, URI))
                {
                    request.Content = new ObjectContent<string>("Steve", new JsonMediaTypeFormatter());

                    var client = ClientFactory.Create(
                        normalizationCallback: (r) => "world peace",
                        requestPayloadHashabilityCallback: (r) => true);

                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    string goodParameter = request.Headers.Authorization.Parameter;

                    // For example, ts="1370846381" becomes ts="bad1370846381"
                    string tampered = goodParameter.Replace((tamperedAttribute + "=\""), (tamperedAttribute + "=\"bad"));

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("hawk", tampered);

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                    }
                }
            }
        }



    }


}
