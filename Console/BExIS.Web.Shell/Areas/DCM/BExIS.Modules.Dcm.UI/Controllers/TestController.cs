using BExIS.UI.Helpers;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index(long id)
        {
            string module = "DCM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);
            ViewData["id"] = id;

            return View();
        }
    }
}