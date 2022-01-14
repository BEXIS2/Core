using BExIS.UI.Helpers;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            // commentar
            SettingsHelper helper = new SettingsHelper("DCM");
            string helpurl = helper.GetValue("help");

            return Redirect(helpurl);
        }
    }
}