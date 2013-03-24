using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebRP.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            if (Request.HttpMethod == "POST")
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        public ActionResult Login()
        {
            var url = FederatedAuthentication.WSFederationAuthenticationModule.CreateSignInRequest(null, null, false).RequestUrl; ;
            return Redirect(url);
        }
        public ActionResult Logout()
        {
            FederatedAuthentication.WSFederationAuthenticationModule.SignOut();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public ActionResult Ajax()
        {
            return Json("Ajax!");
        }
    }
}
