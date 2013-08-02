using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel.Http.Hawk.WebApi;
using Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests.Helpers;

namespace Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests
{
    [TestClass]
    public class HawkValidationFailureTests
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
        public async Task MustReturn401WhenHttpAuthorizationHeaderIsMissing()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                        Assert.IsTrue(String.IsNullOrEmpty(response.Headers.WwwAuthenticate.FirstOrDefault().Parameter));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenSchemeIsNotHawk()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("eagle");

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                        Assert.IsTrue(String.IsNullOrEmpty(response.Headers.WwwAuthenticate.FirstOrDefault().Parameter));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenHawkParameterIsMissing()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("hawk");

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                        Assert.IsTrue(String.IsNullOrEmpty(response.Headers.WwwAuthenticate.FirstOrDefault().Parameter));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenHawkParameterIsInvalid()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("hawk", "invalid parameter");

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                        Assert.IsTrue(String.IsNullOrEmpty(response.Headers.WwwAuthenticate.FirstOrDefault().Parameter));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenHawkParameterHasMissingIdOrNonceOrTsOrMac()
        {
            await MustReturn401WhenHawParameterHasMissingAttribute(missingAttribute: "id");
            await MustReturn401WhenHawParameterHasMissingAttribute(missingAttribute: "nonce");
            await MustReturn401WhenHawParameterHasMissingAttribute(missingAttribute: "ts");
            await MustReturn401WhenHawParameterHasMissingAttribute(missingAttribute: "mac");

        }

        [TestMethod]
        public async Task MustReturn401WhenHawkParameterHasDuplicateAttribute()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var client = ClientFactory.Create();
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    string goodParameter = request.Headers.Authorization.Parameter;
                    string duplicates = goodParameter + "," + goodParameter;

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("hawk", duplicates);

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                        Assert.IsTrue(String.IsNullOrEmpty(response.Headers.WwwAuthenticate.FirstOrDefault().Parameter));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenHawkCredentialIdIsInvalid()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var badCredential = ServerFactory.Credentials.First();
                    badCredential.Id = "SomeIdOtherThan-dh37fgj492je"; // Some id other than dh37fgj492je

                    var client = ClientFactory.Create(() => badCredential);
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                        Assert.IsTrue(String.IsNullOrEmpty(response.Headers.WwwAuthenticate.FirstOrDefault().Parameter));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenHawkParameterTimestampIsStale()
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
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);

                        Assert.IsFalse(String.IsNullOrEmpty(response.Headers.WwwAuthenticate.FirstOrDefault().Parameter));
                        string tsParameter = response.Headers.WwwAuthenticate.FirstOrDefault().Parameter;

                        // ts and tsm must be present
                        Assert.IsTrue(ParameterChecker.IsFieldPresent(tsParameter, "ts"));
                        Assert.IsTrue(ParameterChecker.IsFieldPresent(tsParameter, "tsm"));

                        Assert.IsTrue(await client.AuthenticateAsync(new WebApiResponseMessage(response)));

                    }
                }
            }
        }

        private static async Task MustReturn401WhenHawParameterHasMissingAttribute(string missingAttribute)
        {
            using (var invoker = new HttpMessageInvoker(ServerFactory.Create()))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var client = ClientFactory.Create();
                    await client.CreateClientAuthorizationAsync(new WebApiRequestMessage(request));

                    string goodParameter = request.Headers.Authorization.Parameter;
                    string incompleteParameter = goodParameter.Split(',')
                                            .Where(t => !t.Trim().StartsWith(missingAttribute))
                                                .Aggregate((a, b) => a + "," + b).Trim();

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("hawk", incompleteParameter);

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        Assert.IsNotNull(response.Headers.WwwAuthenticate.FirstOrDefault());
                        Assert.AreEqual("hawk", response.Headers.WwwAuthenticate.FirstOrDefault().Scheme);
                        Assert.IsTrue(String.IsNullOrEmpty(response.Headers.WwwAuthenticate.FirstOrDefault().Parameter));
                    }
                }
            }
        }


        [TestCleanup]
        public void Cleanup()
        {
            if (server != null)
                server.Dispose();
        }
    }
}
