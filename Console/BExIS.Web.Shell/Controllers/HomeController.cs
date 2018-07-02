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

            if (HttpContext.User != null && HttpContext.User.Identity != null && !string.IsNullOrEmpty(HttpContext.User.Identity.Name))
            {
                if (!this.IsAccessibale("DDM", "Home", "Index")) return View();

                var result = this.Render("DDM", "Home", "Index");
                return Content(result.ToHtmlString(), "text/html");
            }

            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Home", this.Session.GetTenant());
            GeneralSettings generalSettings = IoCFactory.Container.Resolve<GeneralSettings>();
            var searchLandingPage = generalSettings.GetEntryValue("searchLandingPage").ToString();
            string[] elements = searchLandingPage.Split(',');
            var moduleName = elements[0].Trim();
            var controllerName = elements[1].Trim();
            var actionName = elements[2].Trim();
            if (!this.IsAccessible(moduleName, controllerName, actionName))
                return View();
            var result = this.Render(moduleName, controllerName, actionName);
            return Content(result.ToHtmlString(), "text/html");
        }


            return View();
        }
        [DoesNotNeedDataAccess]
        public ActionResult SessionTimeout()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Session Timeout", this.Session.GetTenant());

            return View();
        }

    }
}
