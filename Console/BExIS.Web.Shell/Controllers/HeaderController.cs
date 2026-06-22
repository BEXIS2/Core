using System.Web.Mvc;
using System.Web.SessionState;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class HeaderController : Controller
    {
        // GET: Header
        [OutputCache(Duration = 60, VaryByParam = "none")]
        public ActionResult Index()
        {
            return PartialView("Content", Session.GetTenant().HeaderPath);
        }
    }
}