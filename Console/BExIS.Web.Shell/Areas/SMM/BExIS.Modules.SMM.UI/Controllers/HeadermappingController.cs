using BExIS.UI.Helpers;
using System.Web.Mvc;

namespace BExIS.Modules.Smm.UI.Controllers
{
    public class HeadermappingController : Controller
    {
        public ActionResult Index()
        {
            string module = "SMM";
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);
            return View();
        }
    }
}
