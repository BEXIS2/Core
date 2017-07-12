using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class FeaturePermissionsController : Controller
    {
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateFeaturePermissionModel model)
        {
            return View();
        }

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Features", Session.GetTenant());

            var groupManager = new GroupManager();
            groupManager.Create(new Group() { Name = "Public", Description = "Test", GroupType = GroupType.Public });

            var featureManager = new FeatureManager();
            featureManager.Create(new Feature() { Name = "BExIS", Description = "BExIS" });

            var features = new List<FeatureTreeViewItemModel>();

            var roots = featureManager.FindRoots();
            roots.ToList().ForEach(f => features.Add(FeatureTreeViewItemModel.Convert(f)));

            return View(features.AsEnumerable());
        }

        public ActionResult Permissions(long featureId)
        {
            return PartialView("_Permissions", featureId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Permissions_Select(long featureId, GridCommand command)
        {
            var featurePermissionManager = new FeaturePermissionManager();

            // Source + Transformation - Data
            var featurePermissions = featurePermissionManager.FeaturePermissions;
            var total = featurePermissions.Count();

            // Filtering
            var filters = command.FilterDescriptors as List<FilterDescriptor>;

            if (filters != null)
            {
                featurePermissions =
                    featurePermissions.FilterBy<FeaturePermission, FeaturePermissionGridRowModel>(filters);
            }

            // Sorting
            featurePermissions.Sort(command.SortDescriptors);

            // Paging
            featurePermissions = featurePermissions.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);

            // Data
            var data = featurePermissions.ToList().Select(FeaturePermissionGridRowModel.Convert);

            return
                View(new GridModel
                {
                    Data = data,
                    Total = total
                });
        }
    }
}