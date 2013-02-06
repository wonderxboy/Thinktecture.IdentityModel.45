using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace Thinktecture.IdentityModel.Web
{
    public class PassiveRepositorySessionSecurityTokenCache : SessionSecurityTokenCache
    {
        const string Purpose = "PassiveSessionTokenCache";

        ITokenCacheRepositoryFactory factory;
        SessionSecurityTokenCache inner;

        public PassiveRepositorySessionSecurityTokenCache(ITokenCacheRepositoryFactory factory)
            : this(factory, FederatedAuthentication.FederationConfiguration.IdentityConfiguration.Caches.SessionSecurityTokenCache)
        {
        }

        public PassiveRepositorySessionSecurityTokenCache(ITokenCacheRepositoryFactory factory, SessionSecurityTokenCache inner)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (inner == null) throw new ArgumentNullException("inner");

            this.factory = factory;
            this.inner = inner;
        }

        public override void AddOrUpdate(
            SessionSecurityTokenCacheKey key,
            SessionSecurityToken value,
            DateTime expiryTime)
        {
            if (key == null) throw new ArgumentNullException("key");

            inner.AddOrUpdate(key, value, expiryTime);

            var item = new TokenCacheItem
            {
                Key = key.ToString(),
                Expires = expiryTime,
                Token = TokenToBytes(value),
            };
            factory.Create().AddOrUpdate(item);
        }

        public override SessionSecurityToken Get(SessionSecurityTokenCacheKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            var token = inner.Get(key);
            if (token != null) return token;

            var item = factory.Create().Get(key.ToString());
            if (item == null) return null;

            token = BytesToToken(item.Token);

            // update in-mem cache from database
            inner.AddOrUpdate(key, token, item.Expires);

            return token;
        }

        public override void Remove(SessionSecurityTokenCacheKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            inner.Remove(key);
            factory.Create().Remove(key.ToString());
        }

        public override IEnumerable<SessionSecurityToken> GetAll(
            string endpointId, System.Xml.UniqueId contextId)
        {
            throw new NotImplementedException("PassiveRepositorySessionSecurityTokenCache.GetAll");
        }

        public override void RemoveAll(string endpointId)
        {
            throw new NotImplementedException("PassiveRepositorySessionSecurityTokenCache.RemoveAll");
        }

        public override void RemoveAll(string endpointId, System.Xml.UniqueId contextId)
        {
            throw new NotImplementedException("PassiveRepositorySessionSecurityTokenCache.RemoveAll");
        }

        byte[] TokenToBytes(SessionSecurityToken token)
        {
            if (token == null) return null;

            using (var ms = new MemoryStream())
            {
                var f = new BinaryFormatter();
                f.Serialize(ms, token);
                var bytes = ms.ToArray();

                bytes = MachineKey.Protect(bytes, Purpose);

                return bytes;
            }
        }

        SessionSecurityToken BytesToToken(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;

            bytes = MachineKey.Unprotect(bytes, Purpose);

            using (var ms = new MemoryStream(bytes))
            {
                var f = new BinaryFormatter();
                var token = (SessionSecurityToken)f.Deserialize(ms);
                return token;
            }
        }
    }
}
