using System.Web.Mvc;
using Vaiona.Web.Mvc.Data;
using Vaiona.Web.Mvc.Modularity;

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

            return View();
        }
        [DoesNotNeedDataAccess]
        public ActionResult SessionTimeout()
        {
            return View();
        }

    }
}
