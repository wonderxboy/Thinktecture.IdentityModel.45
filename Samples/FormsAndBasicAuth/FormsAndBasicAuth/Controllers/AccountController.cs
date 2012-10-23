using FormsAndBasicAuth.Models;
using System.Web.Mvc;
using System.Web.Security;

namespace FormsAndBasicAuth.Controllers
{
    public class AccountController : Controller
    {
        //
        // POST: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            // check if all required fields are set
            if (ModelState.IsValid)
            {
                // authenticate user
                var success = Membership.ValidateUser(model.UserName, model.Password);

                if (success)
                {
                    // set authentication cookie
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);

                    return RedirectToLocal(returnUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
