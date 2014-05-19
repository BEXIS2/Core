using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.System.Controllers
{
    public class UtilsController : Controller
    {        
        [ChildActionOnly]
        public ActionResult ServerTime(string format)
        {
            return PartialView("_ServerTime", format);
        }

        public ActionResult ServerTime2() // test only
        {
            return PartialView("_ServerTime", "A");
        }

    }
}
