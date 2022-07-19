using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Data;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Controllers
{
    public class TermsAndConditionsController : Controller
    {
        // GET: terms
        [DoesNotNeedDataAccess]
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Terms and conditions", this.Session.GetTenant());

            return View("Index", null, this.Session.GetTenant().TermsAndConditionsFileNamePath);
        }
    }
}