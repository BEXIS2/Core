using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;
using Vaiona.Web.Mvc.Models;
using System.IO;
using System.Xml.Linq;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;
using BExIS.Dlm.Services.Data;
using BExIS.Utils.Helpers;
using System;
using BExIS.Utils.Config;

namespace BExIS.Modules.Bam.UI.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("BAM").GetValueByKey("help").ToString();

            //add default link if not set
            if (String.IsNullOrEmpty(helpurl))
            {
                helpurl = ManualHelper.GetUrl(GeneralSettings.ApplicationVersion, "BAM");
            }

            return Redirect(helpurl);
        }
    }
}