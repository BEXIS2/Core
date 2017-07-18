using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Administration Manual", this.Session.GetTenant());
            return View();
        }
    }
}