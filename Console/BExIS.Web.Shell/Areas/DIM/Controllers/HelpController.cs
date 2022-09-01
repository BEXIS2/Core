using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using Vaiona.Utils.Cfg;
using System.Xml.Linq;
using System.IO;
using BExIS.Xml.Helpers;
using System;
using BExIS.Utils.Helpers;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            string filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), "Dim.Settings.xml");
            XDocument settings = XDocument.Load(filePath);
            XElement help = XmlUtility.GetXElementByAttribute("entry", "key", "help", settings);

            string helpurl = help.Attribute("value")?.Value;


            //add default link if not set
            if (String.IsNullOrEmpty(helpurl))
            {
                helpurl = ManualHelper.GetUrl(AppConfiguration.ApplicationVersion, "DIM");
            }


            return Redirect(helpurl);

        }
    }
}
