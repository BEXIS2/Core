using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using System.IO;
using System.Xml.Linq;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;
using BExIS.Dlm.Services.Data;

namespace BExIS.Modules.Bam.UI.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult Index()
        {
            string filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("BAM"), "Bam.Settings.xml");
            XDocument settings = XDocument.Load(filePath);
            XElement help = XmlUtility.GetXElementByAttribute("entry", "key", "help", settings);

            string helpurl = help.Attribute("value")?.Value;


            return Redirect(helpurl);

        }
    }
}