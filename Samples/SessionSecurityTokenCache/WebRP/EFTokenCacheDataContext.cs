using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Thinktecture.IdentityModel.Web;

namespace WebRP.EF
{
    public class EFTokenCacheDataContext : DbContext
    {
        public EFTokenCacheDataContext()
            :base("name=TokenCache")
        {
        }

        public DbSet<TokenCacheItem> Tokens { get; set; }
    }
}