using System.Web.Mvc;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Controllers
{
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