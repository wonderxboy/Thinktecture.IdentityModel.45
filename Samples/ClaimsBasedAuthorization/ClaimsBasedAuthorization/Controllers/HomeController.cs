using System.Web.Mvc;
using Thinktecture.IdentityModel.Authorization.Mvc;

namespace ClaimsBasedAuthorization.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        [ClaimsAuthorize("View", "About")]
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [ClaimsAuthorize("View", "StreetAddress", "TelephoneNumber")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
