using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace Thinktecture.IdentityModel.Tokens
{
    class MachineKeyMetadataCache : IMetadataCache
    {
        const string Purpose = "MachineKeyMetadataCache";

        IMetadataCache inner;
        public MachineKeyMetadataCache(IMetadataCache inner)
        {
            if (inner == null) throw new ArgumentNullException("inner");

            this.inner = inner;
        }

        public TimeSpan Age
        {
            get { return inner.Age; }
        }

        public byte[] Load()
        {
            var bytes = inner.Load();
            try
            {
                return MachineKey.Unprotect(bytes, Purpose);
            }
            catch
            {
                inner.Save(null);
            }
            return null;
        }

        public void Save(byte[] data)
        {
            data = MachineKey.Protect(data, Purpose);
            inner.Save(data);
        }
    }
}
