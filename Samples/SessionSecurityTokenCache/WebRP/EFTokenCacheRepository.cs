using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Thinktecture.IdentityModel.Web;

namespace WebRP.EF
{
public class EFTokenCacheRepository : ITokenCacheRepository
{
    public void AddOrUpdate(TokenCacheItem item)
    {
        using (EFTokenCacheDataContext db = new EFTokenCacheDataContext())
        {
            DbSet<TokenCacheItem> items = db.Set<TokenCacheItem>();
            var dbItem = items.Find(item.Key);
            if (dbItem == null)
            {
                dbItem = new TokenCacheItem();
                dbItem.Key = item.Key;
                items.Add(dbItem);
            }
            dbItem.Token = item.Token;
            dbItem.Expires = item.Expires;
            db.SaveChanges();
        }
    }

    public TokenCacheItem Get(string key)
    {
        using (EFTokenCacheDataContext db = new EFTokenCacheDataContext())
        {
            DbSet<TokenCacheItem> items = db.Set<TokenCacheItem>();
            return items.Find(key);
        }
    }

    public void Remove(string key)
    {
        using (EFTokenCacheDataContext db = new EFTokenCacheDataContext())
        {
            DbSet<TokenCacheItem> items = db.Set<TokenCacheItem>();
            var item = items.Find(key);
            if (item != null)
            {
                items.Remove(item);
                db.SaveChanges();
            }
        }
    }

    public void RemoveAllBefore(DateTime date)
    {
        using (EFTokenCacheDataContext db = new EFTokenCacheDataContext())
        {
            DbSet<TokenCacheItem> items = db.Set<TokenCacheItem>();
            var query =
                from item in items
                where item.Expires <= date
                select item;
            foreach (var item in query)
            {
                items.Remove(item);
            }
            db.SaveChanges();
        }
    }
}
}