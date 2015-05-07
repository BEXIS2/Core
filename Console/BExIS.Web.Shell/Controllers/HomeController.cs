using System;
using System.Reflection;
using System.Web.Mvc;

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
