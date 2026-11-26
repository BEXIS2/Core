using System.Web.Mvc;
using System.Web.SessionState;

namespace SessionTimeout.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class JSController : Controller
    {
        // GET: JS
        public ActionResult Index()
        {
            return new EmptyResult();
        }
    }
}