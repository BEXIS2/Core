using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
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
            var featurePermissions = featurePermissionManager.FeaturePermissions.To

            // Filtering
            var filtered = users.Where(ExpressionBuilder.Expression<UserGridRowModel>(command.FilterDescriptors));
            var total = filtered.Count();

            // Sorting
            var sorted = (IQueryable<UserGridRowModel>)filtered.Sort(command.SortDescriptors);

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize);

            return View(new GridModel<UserGridRowModel> { Data = paged.ToList(), Total = total });
        }
    }
}