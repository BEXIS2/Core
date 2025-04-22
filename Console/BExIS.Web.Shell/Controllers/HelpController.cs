using BExIS.UI.Helpers;
using BExIS.Utils.Config;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult Index()
        {
            string module = "Shell";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }


        public ActionResult FAQ()
        {
            string helpurl = GeneralSettings.FAQ.ToString();

            return Redirect(helpurl);
        }
    }
}