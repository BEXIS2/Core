using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Vim.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /VIM/Help/

        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("VIM").GetEntryValue("help").ToString();

            return Redirect(helpurl);
        }
    }
}