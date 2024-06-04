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

            //add default link if not set
            if (String.IsNullOrEmpty(helpurl))
            {
                helpurl = ManualHelper.GetUrl(GeneralSettings.ApplicationVersion, "VIM");
            }

            return Redirect(helpurl);
        }
    }
}