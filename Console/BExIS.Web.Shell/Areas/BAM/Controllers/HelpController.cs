using BExIS.Utils.Config;
using BExIS.Utils.Helpers;
using System;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Bam.UI.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("BAM").GetValueByKey("help").ToString();
            ManualHelper.GetUrl(helpurl);

            return Redirect(ManualHelper.GetUrl(helpurl));
        }
    }
}