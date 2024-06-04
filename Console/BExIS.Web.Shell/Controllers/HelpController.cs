using BExIS.Utils.Config;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult FAQ()
        {
            string helpurl = GeneralSettings.FAQ.ToString();

            return Redirect(helpurl);
        }
    }
}