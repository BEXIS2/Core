using System;
using System.Reflection;
using System.Web.Mvc;
using System.Collections.Generic;

namespace BExIS.Web.Shell.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SessionTimeout()
        {
            return View();
        }
    }
}
