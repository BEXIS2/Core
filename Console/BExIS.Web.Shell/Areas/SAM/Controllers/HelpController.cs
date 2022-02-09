using BExIS.UI.Helpers;
using BExIS.Xml.Helpers;
using System.IO;
using System.Web.Mvc;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
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