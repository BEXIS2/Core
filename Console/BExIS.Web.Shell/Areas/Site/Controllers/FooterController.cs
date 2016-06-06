using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Areas.Site.Controllers
{
    public class FooterController : Controller
    {
        // GET: Site/Footer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ContactUs()
        {

            return View("Content", Session.GetTenant().ContactUsFileNamePath);
        }

        public ActionResult Imprint()
        {
            return View("Content", Session.GetTenant().ImprintFileNamePath);
        }

        public ActionResult Policy()
        {
            return View("Content", Session.GetTenant().PolicyFileNamePath);
        }
    }
}