using System.Web.Mvc;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class PublicSearchController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Public", "Home");
        }
    }
}