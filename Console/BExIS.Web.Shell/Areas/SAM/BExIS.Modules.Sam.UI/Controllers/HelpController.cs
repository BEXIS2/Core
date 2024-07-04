using BExIS.Utils.Config;
using BExIS.Utils.Helpers;
using System;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class HelpController : Controller
    {
        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("SAM").GetValueByKey("help").ToString();

            //add default link if not set
            if (String.IsNullOrEmpty(helpurl))
            {
                helpurl = ManualHelper.GetUrl(GeneralSettings.ApplicationVersion, "SAM");
            }

            return Redirect(helpurl);
        }
    }
}