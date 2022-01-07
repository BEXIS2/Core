using BExIS.UI.Helpers;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult FAQ()
        {
            //string filePath = Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "General.Settings.xml");
            //XDocument settings = XDocument.Load(filePath);
            //XElement help = XmlUtility.GetXElementByAttribute("entry", "key", "faq", settings);

            //string helpurl = help.Attribute("value")?.Value;

            SettingsHelper settingsHelper = new SettingsHelper("shell");
            string helpurl = settingsHelper.GetValue("faq");


            return Redirect(helpurl);
        }
    }
}