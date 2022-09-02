using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class HelpController : Controller
    {
        public ActionResult Index()
        {
            var moduleInfo = ModuleManager.GetModuleInfo("SAM");
            string helpurl = ModuleManager.GetModuleSettings("SAM").GetEntryValue("help").ToString();

            return Redirect(helpurl);
        }
    }
}