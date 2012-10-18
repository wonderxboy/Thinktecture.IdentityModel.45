using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Security.Claims;

namespace Thinktecture.IdentityModel.Tests.Misc
{
    [TestClass]
    public class PrincipalTests
    {
        [TestMethod]
        public void CreateAnonymousPrincipal()
        {
            var ap = Principal.Anonymous;

            Assert.IsFalse(ap.Identity.IsAuthenticated);
            Assert.AreEqual<string>("", ap.Identity.Name);
        }

        [TestMethod]
        public void CreateAuthenticatedPrincipal()
        {
            var p = Principal.Create("Test",
                new Claim(ClaimTypes.Name, "test"));

            Assert.IsTrue(p.Identity.IsAuthenticated);
            Assert.AreEqual<string>(p.Identities.First().AuthenticationType, "Test");
            Assert.AreEqual<int>(p.Identities.Count(), 1);
            Assert.AreEqual<int>(p.Identities.First().Claims.Count(), 1);
        }

        [TestMethod]
        public void CreateAuthenticatedPrincipalWithRoles()
        {
            var p = Principal.Create("Test",
                new Claim(ClaimTypes.Name, "test"));

            var roles = Principal.CreateRoles(new string[] { "foo", "bar" });
            p.Identities.First().AddClaims(roles);

            Assert.IsTrue(p.Identity.IsAuthenticated);
            Assert.AreEqual<string>(p.Identities.First().AuthenticationType, "Test");
            Assert.AreEqual<int>(p.Identities.Count(), 1);
            Assert.AreEqual<int>(p.Identities.First().Claims.Count(), 3);
        }

        [TestMethod]
        public void CreateAuthenticatedPrincipalWithEmptyRoles()
        {
            var p = Principal.Create("Test",
                new Claim(ClaimTypes.Name, "test"));

            var roles = Principal.CreateRoles(new string[] { });
            p.Identities.First().AddClaims(roles);

            Assert.IsTrue(p.Identity.IsAuthenticated);
            Assert.AreEqual<string>(p.Identities.First().AuthenticationType, "Test");
            Assert.AreEqual<int>(p.Identities.Count(), 1);
            Assert.AreEqual<int>(p.Identities.First().Claims.Count(), 1);
        }
    }
}
