using BExIS.Utils.Config;
using BExIS.Utils.Helpers;
using System;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /ddm/Help/

        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("DDM").GetValueByKey("help").ToString();
            return Redirect(ManualHelper.GetUrl(helpurl));
        }
    }
}