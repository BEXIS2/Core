using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using Vaiona.Utils.Cfg;
using System.IO;
using System.Xml.Linq;
using BExIS.Xml.Helpers;
using BExIS.UI.Helpers;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            SettingsHelper settingsHelper = new SettingsHelper("RPM");
            string helpurl = settingsHelper.GetValue("help");

            return Redirect(helpurl);


        }
    }
}
