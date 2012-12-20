using System.Web.Mvc;
using Thinktecture.Samples.Controller;

namespace WebHost.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Web API security sample.";

            return View();
        }

        [Authorize]
        public ActionResult IdentityMvc()
        {
            var claims = new IdentityController().Get();
            return View(claims);
        }

        [Authorize]
        public ActionResult IdentityApi()
        {
            return View();
        }
    }
}
