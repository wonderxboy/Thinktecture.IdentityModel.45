using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace WinStoreClient
{
    public class Identity
    {
        public string Name { get; set; }
        public string AuthenticationType { get; set; }
        public bool IsAuthenticated { get; set; }
        public string ClrType { get; set; }

        public ClaimsDto Claims { get; set; }


    }

    public class ClaimsDto : List<ClaimDto>
    {
        public ClaimsDto()
        { }

        public ClaimsDto(IEnumerable<ClaimDto> claims)
            : base(claims)
        { }
    }

    public class ClaimDto
    {
        public string ClaimType { get; set; }
        public string Value { get; set; }
        public string Issuer { get; set; }
        public string OriginalIssuer { get; set; }
    }
}
