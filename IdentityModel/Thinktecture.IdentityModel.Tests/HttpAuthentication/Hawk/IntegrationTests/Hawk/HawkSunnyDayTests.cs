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
using Thinktecture.IdentityModel.Http.Hawk.Core.Extensions;
using Thinktecture.IdentityModel.Http.Hawk.Core.MessageContracts;
using Thinktecture.IdentityModel.Http.Hawk.WebApi;
using Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests.Helpers;

namespace Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests
{
    [TestClass]
    public class HawkSunnyDayTests
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

        [TestCleanup()]
        public void Cleanup()
        {
            typeof(HawkClient).GetProperty("CompensatorySeconds").SetValue(null, 0);
        }

        [TestMethod]
        public async Task MustReturn200WhenValidHawkSchemeHeaderIsPresentInGet()
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
                        Assert.AreEqual("Thanks for flying Hawk", await response.Content.ReadAsAsync<string>());
                        Assert.IsTrue(await client.AuthenticateAsync(new WebApiResponseMessage(response)));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn200ForValidHawkSchemeWithHostRequestHeaderContainingHostNameAndPort()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    request.Headers.Host = "server:99";

                    var client = ClientFactory.Create();
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                        Assert.AreEqual("Thanks for flying Hawk", await response.Content.ReadAsAsync<string>());
                        Assert.IsTrue(await client.AuthenticateAsync(new WebApiResponseMessage(response)));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn200ForValidHawkSchemeWithXffRequestHeaderContainingIPV6AddressAndPort()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    request.Headers.Add("X-Forwarded-For", "[ipv6]:99");

                    var client = ClientFactory.Create();
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                        Assert.AreEqual("Thanks for flying Hawk", await response.Content.ReadAsAsync<string>());
                        Assert.IsTrue(await client.AuthenticateAsync(new WebApiResponseMessage(response)));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn200WhenValidHawkSchemeHeaderIsPresentInGetWithQueryString()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI + "?firstname=Louis&&lastname=Phillipe"))
                {
                    var client = ClientFactory.Create();
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                        Assert.AreEqual("Hello, Louis Phillipe. Thanks for flying Hawk", await response.Content.ReadAsAsync<string>());
                        Assert.IsTrue(await client.AuthenticateAsync(new WebApiResponseMessage(response)));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn200WhenValidHawkSchemeHeaderWithAppSpecifiDataIsPresentInGetAndServerSendsAppSpecificData()
        {
            Func<IResponseMessage, string> normalizationCallback = (response) =>
            {
                // Simulate service adding a sensitive header
                response.AddHeader("X-Header-To-Protect", "Swoop"); // Sensitive header to protect

                // return the status code and the sensitive header in the agreed format
                return (int)response.StatusCode + ":X-Header-To-Protect:Swoop";
            };

            Func<IRequestMessage, string, bool> verificationCallback = (request, appSpecificData) =>
            {
                // Client and server decided that appSpecificData will be in the form of
                // header name and header value separated by a colon, like so: X-Header-To-Protect:Swoosh.

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

            server = ServerFactory.Create(normalizationCallback, verificationCallback);

            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    request.Headers.Add("X-Header-To-Protect", "Swoosh"); // Sensitive header to protect

                    string appSpecificData = String.Empty;
                    Func<IResponseMessage, string, bool> clientVerificationCallback = (r, s) =>
                        {
                            appSpecificData = s;
                            return true;
                        };

                    var client = ClientFactory.Create(
                        normalizationCallback: (r) => "X-Header-To-Protect:Swoosh",
                        verificationCallback: clientVerificationCallback); // Put the sensitive header in the format agreed

                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                        Assert.AreEqual("Thanks for flying Hawk", await response.Content.ReadAsAsync<string>());
                        Assert.IsTrue(await client.AuthenticateAsync(new WebApiResponseMessage(response)));

                        Assert.IsNotNull(appSpecificData);
                        Assert.IsFalse(String.IsNullOrWhiteSpace(appSpecificData));

                        var parts = appSpecificData.Split(':');

                        HttpStatusCode status = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), parts[0]);
                        string headerName = parts[1];
                        string value = parts[2];

                        Assert.AreEqual(response.StatusCode, status);
                        Assert.IsTrue(response.Headers.Contains(headerName));
                        Assert.IsTrue(response.Headers.GetValues(headerName).First().Equals(value));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn200WhenValidHawkSchemeHeaderIsPresentInPost()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, URI))
                {
                    var client = ClientFactory.Create(requestPayloadHashabilityCallback: (r) => true);
                    
                    request.Content = new ObjectContent<string>("Steve", new JsonMediaTypeFormatter());

                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    string parameter = request.Headers.Authorization.Parameter;

                    // hash must be present, since POST contains a request body
                    // and that we specify requestPayloadHashabilityCallback
                    Assert.IsTrue(ParameterChecker.IsFieldPresent(parameter, "hash"));

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("hawk", parameter);

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                        Assert.AreEqual("Hello, Steve. Thanks for flying Hawk", await response.Content.ReadAsAsync<string>());
                        Assert.IsTrue(await client.AuthenticateAsync(new WebApiResponseMessage(response)));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn204WhenValidHawkSchemeHeaderIsPresentInPut()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Put, URI))
                {
                    var client = ClientFactory.Create(requestPayloadHashabilityCallback: (r) => true);

                    request.Content = new ObjectContent<string>("Steve", new JsonMediaTypeFormatter());
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    string parameter = request.Headers.Authorization.Parameter;

                    // hash must be present, since PUT contains a request body
                    // and that we specify requestPayloadHashabilityCallback
                    Assert.IsTrue(ParameterChecker.IsFieldPresent(parameter, "hash"));

                    request.Headers.Authorization = new AuthenticationHeaderValue("hawk", parameter);

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

                        string authorization = response.Headers.GetValues("Server-Authorization").FirstOrDefault();

                        // mac must be present in Server-Authorization
                        Assert.IsTrue(ParameterChecker.IsFieldPresent(authorization, "mac"));

                        // Since there is no response body, no hash must be present in Server-Authorization
                        Assert.IsFalse(ParameterChecker.IsFieldPresent(authorization, "hash"));

                        Assert.IsTrue(await client.AuthenticateAsync(new WebApiResponseMessage(response)));
                    }
                }
            }
        }

        [TestMethod]
        public async Task TimestampInSubsequentRequestMustBeAdjustedBasedOnTimestampInWwwAuthenticateHeaderOfPrevious()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var client = ClientFactory.Create();
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        // Simulate server clock running 5 minutes slower than client and 
                        // produce the parameter with ts and tsm
                        var timestamp = new NormalizedTimestamp(DateTime.UtcNow.AddMinutes(-5), ServerFactory.DefaultCredential);
                        string timestampHeader = timestamp.ToWwwAuthenticateHeaderParameter();

                        // Add that to the WWW-Authenticate before authenticating with the
                        // client, to simulate clock skew
                        response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("hawk", timestampHeader));

                        // Client must have now calculated an offset of -300 seconds approx
                        Assert.IsTrue(await client.AuthenticateAsync(new WebApiResponseMessage(response)));
                        Assert.IsTrue(HawkClient.CompensatorySeconds <= -299 && HawkClient.CompensatorySeconds >= -301);

                        // Create a fresh request and see if this offset is applied to the timestamp
                        using (var subsequentRequest = new HttpRequestMessage(HttpMethod.Get, URI))
                        {
                            await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(subsequentRequest));

                            string header = subsequentRequest.Headers.Authorization.Parameter;

                            ArtifactsContainer artifacts = null;
                            Assert.IsTrue(ArtifactsContainer.TryParse(header, out artifacts));

                            var timestampInSubsequentRequest = artifacts.Timestamp;
                            var now = DateTime.UtcNow.ToUnixTime();

                            // Since server clock is slow, the timestamp going out must be offset by the same 5 minutes
                            // or 300 seconds. Give leeway of a second while asserting.
                            ulong difference = now - timestampInSubsequentRequest;
                            Assert.IsTrue(difference >= 299 && difference <= 301);
                        }
                    }
                }
            }
        }
    }
}
