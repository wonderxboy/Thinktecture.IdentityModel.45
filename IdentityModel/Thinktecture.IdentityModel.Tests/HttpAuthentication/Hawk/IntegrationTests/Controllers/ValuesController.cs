using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        public string Get()
        {
            return "Thanks for flying Hawk";
        }

        public string Get(string firstName, string lastName)
        {
            return String.Format("Hello, {0} {1}. Thanks for flying Hawk", firstName, lastName);
        }

        public void Get(int id) { }

        public string Post([FromBody]string name)
        {
            return String.Format("Hello, {0}. Thanks for flying Hawk", name);
        }

        public void Put() { }
    }
}
