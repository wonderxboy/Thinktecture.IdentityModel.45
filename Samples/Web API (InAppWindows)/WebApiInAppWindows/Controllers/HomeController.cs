using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WebApiInAppWindows.Controllers.Api;
using WebApiInAppWindows.Security;

namespace WebApiInAppWindows.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [Authorize]
        public ActionResult IdentityMvc()
        {
            var claims = new IdentityController().Get();

            ViewBag.PrincipalType = ClaimsPrincipal.Current.GetType().FullName;
            ViewBag.IdentityType = ClaimsPrincipal.Current.Identity.GetType().FullName;

            return View(claims);
        }

        [Authorize]
        public ActionResult IdentityApi()
        {
            ViewBag.Message = "client identity.";

            return View();
        }

        public ActionResult UseApi()
        {
            // custom principal implementation style
            var p = CustomClaimsPrincipal.Current;
            var op = p.IsOperator;

            // extension method style
            op = ClaimsPrincipal.Current.IsOperator();

            return new ContentResult { Content = op.ToString() };
        }
    }
}
