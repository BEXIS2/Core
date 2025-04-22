using BExIS.Utils.Config;
using BExIS.Utils.Helpers;
using System;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Vim.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /VIM/Help/

        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("VIM").GetValueByKey("help").ToString();
            return Redirect(ManualHelper.GetUrl(helpurl));
        }
    }
}