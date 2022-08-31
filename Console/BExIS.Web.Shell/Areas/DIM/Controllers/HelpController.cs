using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("DIM").GetEntryValue("help").ToString();

            return Redirect(helpurl);
        }
    }
}