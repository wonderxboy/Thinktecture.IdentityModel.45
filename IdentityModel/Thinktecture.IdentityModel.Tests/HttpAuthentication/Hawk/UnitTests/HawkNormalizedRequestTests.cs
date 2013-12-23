using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.WebApi;

namespace Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.UnitTests
{
    [TestClass]
    public class HawkNormalizedRequestTests
    {
        [TestMethod]
        public void HostAndPortMustMatchWhatIsInRequestWhenHostAndXffHeadersAreAbsent()
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://server/api/values");

            var normalizedRequest = new NormalizedRequest(new WebApiRequestMessage(request), null);

            PrivateObject po = new PrivateObject(normalizedRequest);

            var hostName = (string)po.GetField("hostName");
            var port = (string)po.GetField("port");

            Assert.AreEqual("server", hostName);
            Assert.AreEqual("80", port);
        }

        [TestMethod]
        public void HttpsMustSetPortTo443()
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://server/api/values");

            var normalizedRequest = new NormalizedRequest(new WebApiRequestMessage(request), null);

            PrivateObject po = new PrivateObject(normalizedRequest);

            var port = (string)po.GetField("port");

            Assert.AreEqual("443", port);
        }

        [TestMethod]
        public void HostAndPortMustMatchWhatIsInHostHeaderWhenPresent()
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://server/api/values");
            request.Headers.Host = "myhost:899";

            var normalizedRequest = new NormalizedRequest(new WebApiRequestMessage(request), null);

            PrivateObject po = new PrivateObject(normalizedRequest);

            var hostName = (string)po.GetField("hostName");
            var port = (string)po.GetField("port");

            Assert.AreEqual("myhost", hostName);
            Assert.AreEqual("899", port);
        }

        [TestMethod]
        public void PortMustDefaultTo80WhenHostHeaderDoesNotContainPort()
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://server/api/values");
            request.Headers.Host = "myhost";

            var normalizedRequest = new NormalizedRequest(new WebApiRequestMessage(request), null);

            PrivateObject po = new PrivateObject(normalizedRequest);

            var hostName = (string)po.GetField("hostName");
            var port = (string)po.GetField("port");

            Assert.AreEqual("myhost", hostName);
            Assert.AreEqual("80", port);
        }

        [TestMethod]
        public void PortMustDefaultTo443ForHttpsWhenHostHeaderDoesNotContainPort()
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://server/api/values");
            request.Headers.Host = "myhost";

            var normalizedRequest = new NormalizedRequest(new WebApiRequestMessage(request), null);

            PrivateObject po = new PrivateObject(normalizedRequest);

            var port = (string)po.GetField("port");

            Assert.AreEqual("443", port);
        }

        [TestMethod]
        public void HostAndPortMustMatchWhatIsInXffHeaderWhenPresent()
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://server/api/values");
            request.Headers.Host = "myhost:899";
            request.Headers.Add("X-Forwarded-For", "xffhost:4444");

            var normalizedRequest = new NormalizedRequest(new WebApiRequestMessage(request), null);

            PrivateObject po = new PrivateObject(normalizedRequest);

            var hostName = (string)po.GetField("hostName");
            var port = (string)po.GetField("port");

            Assert.AreEqual("xffhost", hostName);
            Assert.AreEqual("4444", port);
        }

        [TestMethod]
        public void HostAndPortMustMatchWhatIsInXffHeaderWhenPresentContainingIpv6()
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://server/api/values");
            request.Headers.Host = "myhost:899";
            request.Headers.Add("X-Forwarded-For", "[111:111:111]:4444");

            var normalizedRequest = new NormalizedRequest(new WebApiRequestMessage(request), null);

            PrivateObject po = new PrivateObject(normalizedRequest);

            var hostName = (string)po.GetField("hostName");
            var port = (string)po.GetField("port");

            Assert.AreEqual("[111:111:111]", hostName);
            Assert.AreEqual("4444", port);
        }

        [TestMethod]
        public void HostAndPortMustMatchWhatIsInTheFirstXffHeaderWhenMultipleXffHeadersArePresent()
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://server/api/values");
            request.Headers.Host = "myhost:899";
            request.Headers.Add("X-Forwarded-For", "xffhost1:1111");
            request.Headers.Add("X-Forwarded-For", "xffhost2:2222"); // Same as "xffhost1:1111, xffhost2:2222"

            var normalizedRequest = new NormalizedRequest(new WebApiRequestMessage(request), null);

            PrivateObject po = new PrivateObject(normalizedRequest);

            var hostName = (string)po.GetField("hostName");
            var port = (string)po.GetField("port");

            Assert.AreEqual("xffhost1", hostName);
            Assert.AreEqual("1111", port);
        }

        [TestMethod]
        public void HostAndPortMustMatchWhatIsInTheFirstXffHeaderWhenMultipleXffHeadersArePresentWithIpv6Address()
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://server/api/values");
            request.Headers.Host = "myhost:899";
            request.Headers.Add("X-Forwarded-For", "[111:111:111]:1111");
            request.Headers.Add("X-Forwarded-For", "[222:222:222]:2222"); // Same as "[111:111:111]:1111, [222:222:222]:2222"

            var normalizedRequest = new NormalizedRequest(new WebApiRequestMessage(request), null);

            PrivateObject po = new PrivateObject(normalizedRequest);

            var hostName = (string)po.GetField("hostName");
            var port = (string)po.GetField("port");

            Assert.AreEqual("[111:111:111]", hostName);
            Assert.AreEqual("1111", port);
        }
    }
}
