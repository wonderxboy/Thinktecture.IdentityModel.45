using System;
using System.ComponentModel.DataAnnotations;

namespace Thinktecture.IdentityModel.Web
{
    public interface IReplayTokenCacheRepository
    {
        void AddOrUpdate(ReplayTokenCacheItem item);
        bool Contains(string key);
        void Remove(string key);
        void RemoveAllBefore(DateTime date);
    }

    public class ReplayTokenCacheItem
    {
        [Key]
        public string Key { get; set; }
        public DateTime Expires { get; set; }
    }
}