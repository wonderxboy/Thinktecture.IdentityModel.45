using System.Web.Mvc;

namespace FormsAndBasicAuth.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        [Authorize]
        public ActionResult Greeting()
        {
            ViewBag.Message = "Greeting page.";
            ViewBag.Client = User.Identity.Name;

            return View();
        }
    }
}
