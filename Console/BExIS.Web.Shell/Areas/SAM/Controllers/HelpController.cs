using BExIS.Utils.Helpers;
using BExIS.Xml.Helpers;
using System;
using System.IO;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class HelpController : Controller
    {
        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("SAM").GetEntryValue("help").ToString();

            //add default link if not set
            if (String.IsNullOrEmpty(helpurl))
            {
                helpurl = ManualHelper.GetUrl(AppConfiguration.ApplicationVersion, "SAM");
            }
      
           

            return Redirect(helpurl);
        }
    }
}