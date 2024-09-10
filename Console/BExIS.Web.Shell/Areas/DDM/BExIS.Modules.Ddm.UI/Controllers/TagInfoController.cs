using BExIS.UI.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class TagInfoController : BaseController
    {
        // GET: TagInfo
        public ActionResult Index(long id)
        {
            string module = "DDM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);
            ViewData["id"] = id;

            return View();
        }

        public ActionResult UpdateSearch(long id)
        {
            if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
            {
                return this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", id } });
            }

            return null;
        }

    }
}