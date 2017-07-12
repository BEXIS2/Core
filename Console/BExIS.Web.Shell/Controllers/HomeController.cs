using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

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
