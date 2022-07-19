using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using System.Xml.Linq;
using System.IO;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Vim.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /VIM/Help/

        public ActionResult Index()
        {
            string filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("VIM"), "Vim.Settings.xml");
            XDocument settings = XDocument.Load(filePath);
            XElement help = XmlUtility.GetXElementByAttribute("entry", "key", "help", settings);

            string helpurl = help.Attribute("value")?.Value;


            return Redirect(helpurl);

        }
    }
}
