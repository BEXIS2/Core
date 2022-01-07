using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.UI.Helpers;
using BExIS.Xml.Helpers;
using System.IO;
using System.Web.Mvc;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            SettingsHelper helper = new SettingsHelper("DCM");
            string helpurl = helper.GetValue("help");

            return Redirect(helpurl);
        }
    }
}