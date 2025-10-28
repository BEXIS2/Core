using BExIS.Utils.Helpers;
using System;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class HelpController : Controller
    {
  
        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("RPM").GetValueByKey("help").ToString();
            return Redirect(ManualHelper.GetUrl(helpurl));
        }
    }
}