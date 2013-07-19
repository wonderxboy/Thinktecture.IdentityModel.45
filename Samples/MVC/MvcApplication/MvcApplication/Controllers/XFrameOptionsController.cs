using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Web.Mvc;

namespace MvcApplication.Controllers
{
    public class XFrameOptionsController : Controller
    {
        //
        // GET: /XFrameOptions/

        public ActionResult Index()
        {
            return View();
        }

        [FrameOptions]
        public ActionResult DenyImplicit()
        {
            return View();
        }
        
        [FrameOptions(FrameOptions.Deny)]
        public ActionResult Deny()
        {
            return View();
        }

        [FrameOptions(FrameOptions.SameOrigin)]
        public ActionResult SameOrigin()
        {
            return View();
        }

        [FrameOptions("http://localhost:23626")]
        public ActionResult CustomOrigin1()
        {
            return View();
        }

        [FrameOptions("http://foo.com")]
        public ActionResult CustomOrigin2()
        {
            return View();
        }

        [MyDynamicFrameOptions]
        public ActionResult CustomDynamic()
        {
            return View();
        }

    }

    public class MyDynamicFrameOptionsAttribute : FrameOptionsAttribute
    {
        public MyDynamicFrameOptionsAttribute()
            : base(FrameOptions.CustomOrigin)
        {
        }

        protected override string GetCustomOrigin(HttpRequestBase request)
        {
            // do your DB lookup here
            if (request.Url.Host == "someHostITrust" || request.Url.Host == "localhost")
            {
                var origin =
                    request.Url.Scheme +
                    "://" +
                    request.Url.Host + (request.Url.Port == 80 ? "" : ":" + request.Url.Port);
                return origin;
            }
            return null;
        }
    }

}
