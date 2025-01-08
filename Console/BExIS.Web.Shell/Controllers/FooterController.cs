using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Controllers
{
    public class FooterController : Controller
    {
        // GET: footer
        [OutputCache(Duration = 60, VaryByParam = "none")]
        public ActionResult Index()
        {
            return PartialView("Content", Session.GetTenant().FooterPath);
        }

        [OutputCache(Duration = 60, VaryByParam = "none")]
        public ActionResult ContactUs()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Contact Us", this.Session.GetTenant());
            return View("Content", null, Session.GetTenant().ContactUsFileNamePath);
        }

        [OutputCache(Duration = 60, VaryByParam = "none")]
        public ActionResult Imprint()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Imprint", this.Session.GetTenant());
            return View("Content", null, Session.GetTenant().ImprintFileNamePath);
        }

        [OutputCache(Duration = 60, VaryByParam = "none")]
        public ActionResult TermsAndConditions()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("TermsAndConditions", this.Session.GetTenant());
            return View("Content", null, this.Session.GetTenant().TermsAndConditionsFileNamePath);
        }

        [OutputCache(Duration = 60, VaryByParam = "none")]
        public ActionResult Policy()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Policy", this.Session.GetTenant());
            return View("Content", null, this.Session.GetTenant().PolicyFileNamePath);
        }
    }
}