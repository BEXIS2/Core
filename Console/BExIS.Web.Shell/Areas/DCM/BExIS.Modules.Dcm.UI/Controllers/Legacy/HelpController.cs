using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            // commentar
            string helpurl = ModuleManager.GetModuleSettings("DCM").GetValueByKey("help").ToString();

            return Redirect(helpurl);
        }
    }
}