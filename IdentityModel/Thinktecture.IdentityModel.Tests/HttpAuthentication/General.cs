using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;

namespace Tests
{
    [TestClass]
    public class General
    {
        [TestMethod]
        public void NoCredential()
        {
            var client = new HttpClient(Factory.GetDefaultServer());
            var request = Factory.GetDefaultRequest();

            var response = client.SendAsync(request).Result;
            
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
