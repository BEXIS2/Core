using System.Net.NetworkInformation;
using System.Web.Mvc;
using System.Windows.Forms;

namespace BExIS.Web.Shell.Areas.Site.Controllers
{
    public class NavController : Controller
    {
        [ChildActionOnly]
        public ActionResult Menu(int oreintation)
        {
            return PartialView("_Menu"); // it should be possible to get the view from the current theme's Partials folder. PartialViewThemed, Core.UI
        }

        public ActionResult Menu2() // for ajax menu test. it should be deleted
        {
            return PartialView("_Menu"); // it should be possible to get the view from the current theme's Partials folder. PartialViewThemed, Core.UI
        }
    }
}
