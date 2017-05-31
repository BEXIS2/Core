using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Authorization;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class EntityPermissionsController : Controller
    {
        [GridAction]
        public ActionResult EntityPermissions_Select(GridActionAttribute filters)
        {
            var entityPermissionManager = new EntityPermissionManager();
            var entityPermissions = entityPermissionManager.EntityPermissions.Select(e => EntityPermissionGridRowModel.Convert(e)).ToList();

            return View(new GridModel<EntityPermissionGridRowModel> { Data = entityPermissions });
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}