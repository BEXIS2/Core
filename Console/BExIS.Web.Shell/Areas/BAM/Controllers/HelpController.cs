using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Bam.UI.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("BAM").GetEntryValue("help").ToString();

            return Redirect(helpurl);
        }
    }
}