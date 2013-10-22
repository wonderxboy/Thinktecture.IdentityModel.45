using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel.Http.Hawk.Client;
using Thinktecture.IdentityModel.Http.Hawk.Core.Extensions;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Http.Hawk.WebApi;
using Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests.Helpers;

namespace Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests
{
    [TestClass]
    public class HawkBewitTests
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
        public async Task MustReturn200WhenBewitIsValid()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var client = ClientFactory.Create();
                    client.CreateBewit(new WebApiRequestMessage(request), 10);

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                        Assert.AreEqual("Thanks for flying Hawk", await response.Content.ReadAsAsync<string>());
                        Assert.IsFalse(response.Headers.Contains(HawkConstants.ServerAuthorizationHeaderName));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn200WhenBewitIsValidAndContainsAppSpecificData()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var client = ClientFactory.Create(normalizationCallback: (r) => "Humpty Dumpty");
                    client.CreateBewit(new WebApiRequestMessage(request), 10);

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                        Assert.AreEqual("Thanks for flying Hawk", await response.Content.ReadAsAsync<string>());
                        Assert.IsFalse(response.Headers.Contains(HawkConstants.ServerAuthorizationHeaderName));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn200WhenUriHasQueryStringAndBewitIsValid()
        {
            string uriWithQueryString = URI + "?firstname=john";           

            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, uriWithQueryString))
                {
                    var client = ClientFactory.Create();
                    client.CreateBewit(new WebApiRequestMessage(request), 10);

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                        Assert.AreEqual("Thanks for flying Hawk", await response.Content.ReadAsAsync<string>());
                        Assert.IsFalse(response.Headers.Contains(HawkConstants.ServerAuthorizationHeaderName));
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn200WhenRequestObjectNotReusedAndBewitIsValid()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var client = ClientFactory.Create();
                    string bewit = client.CreateBewit(new WebApiRequestMessage(request), 10);

                    using (var freshRequest = new HttpRequestMessage())
                    {
                        freshRequest.RequestUri = new Uri(URI + "?bewit=" + bewit); // manually add bewit to query string

                        using (var response = await invoker.SendAsync(freshRequest, CancellationToken.None))
                        {
                            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                            Assert.AreEqual("Thanks for flying Hawk", await response.Content.ReadAsAsync<string>());
                            Assert.IsFalse(response.Headers.Contains(HawkConstants.ServerAuthorizationHeaderName));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenBewitHasExpired()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var client = ClientFactory.Create();
                    client.CreateBewit(new WebApiRequestMessage(request), 0); // no life in it

                    using (var response = await invoker.SendAsync(request, CancellationToken.None))
                    {
                        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task MustThrowInvalidOperationExceptionWhenBewitUsedWithPost()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, URI))
                {
                    var client = ClientFactory.Create();
                    client.CreateBewit(new WebApiRequestMessage(request), 10);

                    await invoker.SendAsync(request, CancellationToken.None);
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenBewitIsTampered()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var client = ClientFactory.Create();
                    string bewit = client.CreateBewit(new WebApiRequestMessage(request), 10);

                    var parts = bewit.ToUtf8StringFromBase64Url().Split('\\');
                    string id = parts[0];
                    string timestamp = parts[1];
                    string mac = parts[2];
                    string ext = parts[3];

                    string tamperedBewit = String.Format(@"{0}\{1}\{2}\{3}", "Id of my choice", timestamp, mac, ext);
                    tamperedBewit = tamperedBewit.ToBytesFromUtf8().ToBase64UrlString();

                    using (var freshRequest = new HttpRequestMessage())
                    {
                        freshRequest.RequestUri = new Uri(URI + "?bewit=" + tamperedBewit);

                        using (var response = await invoker.SendAsync(freshRequest, CancellationToken.None))
                        {
                            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task MustReturn401WhenUriIsTampered()
        {
            using (var invoker = new HttpMessageInvoker(server))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, URI))
                {
                    var client = ClientFactory.Create();
                    string bewit = client.CreateBewit(new WebApiRequestMessage(request), 10);

                    var parts = bewit.ToUtf8StringFromBase64Url().Split('\\');
                    string id = parts[0];
                    string timestamp = parts[1];
                    string mac = parts[2];
                    string ext = parts[3];

                    string tamperedBewit = String.Format(@"{0}\{1}\{2}\{3}", "Id of my choice", timestamp, mac, ext);
                    tamperedBewit = tamperedBewit.ToBytesFromUtf8().ToBase64UrlString();

                    using (var freshRequest = new HttpRequestMessage())
                    {
                        string tamperedUri = URI + "/1";
                        freshRequest.RequestUri = new Uri(tamperedUri + "?bewit=" + tamperedBewit);

                        using (var response = await invoker.SendAsync(freshRequest, CancellationToken.None))
                        {
                            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                        }
                    }
                }
            }
        }
    }
}
