using BExIS.Ddm.Api;
using System.Web.Mvc;
using Vaiona.IoC;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            // test Reindex
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
            provider?.Reload();

            return View();
        }
    }
}