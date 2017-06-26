using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Data Planning Manual", this.Session.GetTenant());
            return View();

        }
    }
}
