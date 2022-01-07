using BExIS.UI.Helpers;
using BExIS.Xml.Helpers;
using System.IO;
using System.Web.Mvc;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class HelpController : Controller
    {
        public ActionResult Index()
        {
            SettingsHelper settingsHelper = new SettingsHelper("SAM");
            string helpurl = settingsHelper.GetValue("help");

            return Redirect(helpurl);


        }
    }
}