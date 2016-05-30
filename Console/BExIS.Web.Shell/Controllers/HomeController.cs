using System;
using System.Reflection;
using System.Web.Mvc;
using System.Collections.Generic;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitle(Request.IsAuthenticated ? "Dashboard" : "Home");

            return View();
        }

        public ActionResult SessionTimeout()
        {
            return View();
        }

    }
}
