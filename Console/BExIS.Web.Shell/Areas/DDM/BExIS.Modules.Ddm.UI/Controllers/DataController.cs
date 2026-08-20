using System.Web.Mvc;
using Vaiona.Web.Mvc;


namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class DataController : BaseController
    {
        public ActionResult ShowData(long id, int version = 0, bool asPartial = false, string versionName = "", double tag = 0)
        {
            return Redirect("/dcm/view?id=" + id + "&version=" + version + "&tag=" + tag);
        }
    }
}