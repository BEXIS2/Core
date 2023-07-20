using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using System.Xml.Linq;
using System.IO;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;
using System;
using BExIS.Utils.Helpers;
using Vaiona.Web.Mvc.Modularity;
using BExIS.Utils.Config;

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