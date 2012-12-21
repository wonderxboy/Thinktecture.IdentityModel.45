using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.Samples
{
    public static class Constants
    {
        public const string WebHostName = "roadie";
        public const string SelfHostName = "roadie";
        
        public const string AppName = "/webapisecurity/api/";

        public const string WebHostBaseAddress = "https://" + WebHostName + AppName;
        public const string SelfHostBaseAddress = "https://" + SelfHostName + AppName;
    }
}
