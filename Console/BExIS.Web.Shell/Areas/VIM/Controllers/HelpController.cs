using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using System.Xml.Linq;
using System.IO;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;
using BExIS.UI.Helpers;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Vim.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /VIM/Help/

        public ActionResult Index()
        {
            var moduleInfo = ModuleManager.GetModuleInfo("VIM");
            string helpurl = moduleInfo.Plugin.Settings.GetEntryValue("help").ToString();

            return Redirect(helpurl);
        }
    }
}