using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using System.IO;
using Vaiona.Utils.Cfg;
using System.Xml;
using BExIS.Xml.Helpers;
using System.Xml.Linq;
using Markdig;
using System.Net;
using System.Web;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /ddm/Help/

        public ActionResult Index()
        {

            string filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Ddm.Settings.xml");
            XDocument settings = XDocument.Load(filePath);
            XElement help = XmlUtility.GetXElementByAttribute("entry", "key", "help", settings);

            string helpurl = help.Attribute("value")?.Value;

            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //using (var client = new WebClient())
            //{

            //    var helpfile = client.DownloadString(helpurl);

            //    string model = Markdown.ToHtml(helpfile);
            //    return View("Index", (object)model);
            //}

            return Redirect(helpurl);

        }

    }
}
