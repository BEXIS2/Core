using System.Web.Mvc;
using BExIS.UI.Helpers;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class CurationController : Controller
    {
        public ActionResult Index(long id = 0)
        {
            string module = "DDM";
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);
            ViewData["id"] = id;
            return View();
        }
    } 
}