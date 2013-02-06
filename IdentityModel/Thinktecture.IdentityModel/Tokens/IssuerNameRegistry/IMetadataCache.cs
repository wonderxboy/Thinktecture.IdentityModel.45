using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Thinktecture.IdentityModel.Tokens
{
    public interface IMetadataCache
    {
        TimeSpan Age { get; }
        byte[] Load();
        void Save(byte[] data);
    }
}