using System.Web.Mvc;
using Vaiona.Web.Mvc.Data;
using Vaiona.Web.Mvc.Modularity;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using BExIS.Web.Shell.Helpers;
using Vaiona.IoC;

namespace BExIS.Web.Shell.Controllers
{
    public class HomeController : Controller
    {
        [DoesNotNeedDataAccess]
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Home", this.Session.GetTenant());

            var landingPage = this.Session.GetTenant().LandingPageTuple;
            if (!this.IsAccessible(landingPage.Item1, landingPage.Item2, landingPage.Item3))
                return View();
            var result = this.Render(landingPage.Item1, landingPage.Item2, landingPage.Item3);
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
