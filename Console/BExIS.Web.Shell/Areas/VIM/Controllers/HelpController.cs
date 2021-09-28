using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using System.Xml.Linq;
using System.IO;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;
using BExIS.UI.Helpers;

namespace BExIS.Modules.Vim.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /VIM/Help/

        public ActionResult Index()
        {
            SettingsHelper settingsHelper = new SettingsHelper("VIM");
            string helpurl = settingsHelper.GetValue("help");

            return Redirect(helpurl);


        }
    }
}
