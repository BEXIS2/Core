using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using Vaiona.Utils.Cfg;
using System.IO;
using System.Xml.Linq;
using BExIS.Xml.Helpers;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            string filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Rpm.Settings.xml");
            XDocument settings = XDocument.Load(filePath);
            XElement help = XmlUtility.GetXElementByAttribute("entry", "key", "help", settings);

            string helpurl = help.Attribute("value")?.Value;


            return Redirect(helpurl);

        }
    }
}
