using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.Site.Controllers
{
    public class TermsController : Controller
    {
        // GET: Site/Terms
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Terms and conditions", this.Session.GetTenant());
            return View();
        }
    }
}