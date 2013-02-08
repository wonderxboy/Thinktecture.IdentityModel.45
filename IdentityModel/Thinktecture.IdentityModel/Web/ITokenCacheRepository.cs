using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityModel.Web
{
    public interface ITokenCacheRepository
    {
        void AddOrUpdate(TokenCacheItem item);
        TokenCacheItem Get(string key);
        void Remove(string key);
        void RemoveAllBefore(DateTime date);
    }

    public class TokenCacheItem
    {
        [Key]
        public string Key { get; set; }
        public DateTime Expires { get; set; }
        public byte[] Token { get; set; }
    }
}
