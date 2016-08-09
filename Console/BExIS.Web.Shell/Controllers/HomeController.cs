using System;
using System.Reflection;
using System.Web.Mvc;
using System.Collections.Generic;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant(Request.IsAuthenticated ? "Dashboard" : "Home", this.Session.GetTenant());

            return View();
        }

        public ActionResult SessionTimeout()
        {
            return View();
        }

    }
}
