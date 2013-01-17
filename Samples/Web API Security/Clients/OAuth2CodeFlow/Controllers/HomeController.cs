using System.Web.Mvc;
using Thinktecture.IdentityModel.Clients;
using Thinktecture.Samples;

namespace OAuth2CodeFlow.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var url = OAuth2Client.CreateCodeFlowUrl(
                "https://idsrv.local/issue/oauth2/authorize",
                "codeflowclient",
                Constants.Scope,
                "http://localhost:12345/callback");
            
            ViewBag.AuthorizeUrl = url;

            return View();
        }
    }
}
