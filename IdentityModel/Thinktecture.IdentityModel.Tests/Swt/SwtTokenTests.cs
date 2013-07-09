using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IdentityModel.Tokens;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thinktecture.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Protocols.WSTrust;

namespace Thinktecture.IdentityModel.Tests.Swt
{
    [TestClass]
    public class SwtTokenTests
    {
        [TestMethod]
        public void GetTokenClaimsAsEncodedArrayString()
        {
            byte[] key;
            var token = this.GetToken(out key);
            var builder = new StringBuilder();
            SimpleWebTokenHandler.CreateClaims(token, builder);

            var builderOutput = builder.ToString();
            Assert.AreEqual(builderOutput, "http%3a%2f%2fschemas.microsoft.com%2fws%2f2008%2f06%2fidentity%2fclaims%2frole=Administrator%2cDomain%2bAdministrator%2cSome%252cNotVeryNice%252cEncodedClaim&");
        }

        [TestMethod]
        public void CreateTokenAndParseEncodedMultipleClaims()
        {
            var handler = new SimpleWebTokenHandler();

            byte[] key;
            var token = this.GetToken(out key);
            var tokenString = TokenToString(token);
            var signedToken = handler.ReadToken(new XmlTextReader(new StringReader(tokenString)));

            handler.Configuration = new SecurityTokenHandlerConfiguration();

            var symmetricKey = new InMemorySymmetricSecurityKey(key);
            
            handler.Configuration.AudienceRestriction.AllowedAudienceUris.Add(
                new Uri("http://audience.com"));

            var resolverTable = new Dictionary<string, IList<SecurityKey>>
            {
                { "http://test.com", new SecurityKey[] { symmetricKey } }
            };

            handler.Configuration.IssuerTokenResolver = new NamedKeyIssuerTokenResolver(resolverTable);

            var ids = handler.ValidateToken(signedToken);
            var id = ids.FirstOrDefault();
            
            Assert.IsNotNull(id);

            var testClaims = GetClaims();

            Assert.IsTrue(id.Claims.Count() == 3);
            Assert.IsTrue(id.HasClaim(testClaims[0].Type, testClaims[0].Value));
            Assert.IsTrue(id.HasClaim(testClaims[1].Type, testClaims[1].Value));
            Assert.IsTrue(id.HasClaim(testClaims[2].Type, testClaims[2].Value));
        }

        private static string TokenToString(SecurityToken token)
        {
            var sb = new StringBuilder();

            using (var writer = XmlWriter.Create(sb))
            {
                new SimpleWebTokenHandler().WriteToken(writer, token);
            }

            return sb.ToString();
        }

        private List<Claim> GetClaims()
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Administrator"),
                new Claim(ClaimTypes.Role, "Domain Administrator"),
                new Claim(ClaimTypes.Role, "Some,NotVeryNice,EncodedClaim")
            };
        }


        public byte[] GetKey()
        {
            return CryptoRandom.CreateRandomKey(32);
        }

        public SimpleWebToken GetToken(out byte[] key)
        {
            key = GetKey();

            var descripter = new SecurityTokenDescriptor();
            descripter.Lifetime = new Lifetime(DateTime.Now, DateTime.Now.AddMinutes(5));
            descripter.TokenIssuerName = "http://test.com";
            descripter.SigningCredentials = new HmacSigningCredentials(key);
            descripter.Subject = new ClaimsIdentity(this.GetClaims());
            descripter.AppliesToAddress = "http://audience.com";

            var output = new SimpleWebTokenHandler().CreateToken(descripter) as SimpleWebToken;

            return output;
        }

       
    }
}
