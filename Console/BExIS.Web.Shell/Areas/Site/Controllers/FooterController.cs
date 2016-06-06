using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

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
            ViewBag.Title = PresentationModel.GetViewTitle("Contact Us");
            return View("Content", Session.GetTenant().ContactUsFileNamePath);
        }

        public ActionResult Imprint()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Imprint");
            return View("Content", Session.GetTenant().ImprintFileNamePath);
        }

        public ActionResult Policy()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Policy");
            return View("Content", Session.GetTenant().PolicyFileNamePath);
        }
    }
}