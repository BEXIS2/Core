using BExIS.UI.Helpers;
using System.Web.Mvc;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class MeaningsController : Controller
    {
        // GET: Meanings
        public ActionResult Index()
        {
            string module = "rpm";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }
    }
}