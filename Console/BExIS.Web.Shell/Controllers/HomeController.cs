using System.Web.Mvc;
using Vaiona.Web.Mvc.Data;
using Vaiona.Web.Mvc.Modularity;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Controllers
{
    public class HomeController : Controller
    {
        [DoesNotNeedDataAccess]
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Home", this.Session.GetTenant());

            if (!this.IsAccessible("DDM", "Home", "Index"))
                return View();
            var result = this.Render("DDM", "Home", "Index");
            return Content(result.ToHtmlString(), "text/html");
        }

        [DoesNotNeedDataAccess]
        public ActionResult SessionTimeout()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Session Timeout", this.Session.GetTenant());

            return View();
        }

    }
}
