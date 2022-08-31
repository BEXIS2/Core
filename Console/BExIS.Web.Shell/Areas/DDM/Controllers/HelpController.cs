using System.Web.Mvc;
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