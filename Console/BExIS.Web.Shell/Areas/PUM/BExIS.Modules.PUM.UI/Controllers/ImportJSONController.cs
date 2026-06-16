using System.Web.Mvc;
using BExIS.UI.Helpers;

namespace BExIS.Modules.Pum.UI.Controllers
{
    public class ImportJSONController : Controller
    {
        public ActionResult Index(long id = 0)
        {
            string module = "PUM";
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);
            ViewData["id"] = id;
            return View();
        }
    }
}