using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using System.IO;
using Vaiona.Utils.Cfg;
using System.Xml;
using BExIS.Xml.Helpers;
using System.Xml.Linq;
using System.Net;
using System.Web;
using BExIS.UI.Helpers;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /ddm/Help/

        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("DDM").GetEntryValue("help").ToString();

            return Redirect(helpurl);
        }
    }
}