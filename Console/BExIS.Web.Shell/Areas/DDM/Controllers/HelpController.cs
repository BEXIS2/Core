using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using System.IO;
using Vaiona.Utils.Cfg;
using System.Xml;
using BExIS.Xml.Helpers;
using System.Xml.Linq;
using System.Net;
using System.Web;
using System;
using BExIS.Utils.Helpers;
using BExIS.Utils.Config;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /ddm/Help/

        public ActionResult Index()
        {

            string helpurl = ModuleManager.GetModuleSettings("DDM").GetValueByKey("help").ToString();

            //add default link if not set
            if (String.IsNullOrEmpty(helpurl))
            {
                helpurl = ManualHelper.GetUrl(GeneralSettings.ApplicationVersion, "DDM");
            }
           

            return Redirect(helpurl);
        }
    }
}