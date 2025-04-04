using BExIS.Utils.Config;
using BExIS.Utils.Helpers;
using System;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("DIM").GetValueByKey("help").ToString();
            return Redirect(ManualHelper.GetUrl(helpurl));
        }
    }
}