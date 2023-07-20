using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using Vaiona.Utils.Cfg;
using System.Xml.Linq;
using System.IO;
using BExIS.Xml.Helpers;
using System;
using BExIS.Utils.Helpers;
using BExIS.Utils.Config;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("DIM").GetValueByKey("help").ToString();

            //add default link if not set
            if (String.IsNullOrEmpty(helpurl))
            {
                helpurl = ManualHelper.GetUrl(GeneralSettings.ApplicationVersion, "DIM");
            }


            return Redirect(helpurl);
        }
    }
}