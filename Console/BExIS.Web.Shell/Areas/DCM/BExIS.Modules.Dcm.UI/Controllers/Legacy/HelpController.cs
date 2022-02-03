using BExIS.UI.Helpers;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            // commentar
            var moduleInfo = ModuleManager.GetModuleInfo("DCM");
            string helpurl = moduleInfo.Plugin.Settings.GetEntryValue("help").ToString();

            return Redirect(helpurl);
        }
    }
}