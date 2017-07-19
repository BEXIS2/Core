using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Authorization;
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
        [HttpGet]
        public ActionResult AddSubjects(long featureId)
        {
            var featurePermissionManager = new FeaturePermissionManager();
            var subjects = featurePermissionManager.FeaturePermissions.Where(x => x.Feature.Id == featureId)
                    .Select(x => new KeyValuePair<long, int>(x.Subject.Id, (int)x.PermissionType));

            subjects.ToDictionary(x => x.Key, x => x.Value);

            Session["SelectedSubjects"] = subjects.ToDictionary(x => x.Key, x => x.Value);

            return PartialView("_Add", featureId);
        }

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Features", Session.GetTenant());

            var featureManager = new FeatureManager();

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
            var sorted = featurePermissions.Sort(command.SortDescriptors) as IQueryable<FeaturePermission>;

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);

            // Data
            var data = paged.Select(FeaturePermissionGridRowModel.Convert).ToList();

            return View(new GridModel<FeaturePermissionGridRowModel>
            {
                Data = data,
                Total = total
            });
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Subjects_Select(Dictionary<long, int> selection, long featureId, GridCommand command)
        {
            if (selection == null)
                selection = new Dictionary<long, int>();

            // Selected Groups
            Dictionary<long, int> selectedSubjects = new Dictionary<long, int>();
            if (Session["SelectedSubjects"] != null)
                selectedSubjects = Session["SelectedSubjects"] as Dictionary<long, int>;

            foreach (var item in selection)
            {
                if (item.Value != 2)
                {
                    selectedSubjects[item.Key] = item.Value;
                }
            }

            Session["SelectedSubjects"] = selectedSubjects;

            var subjectManager = new SubjectManager();

            // Source + Transformation - Data
            var subjects = subjectManager.Subjects;
            var total = subjects.Count();

            // Filtering
            var filters = command.FilterDescriptors as List<FilterDescriptor>;

            if (filters != null)
            {
                subjects =
                    subjects.FilterBy<Subject, CreateFeaturePermissionGridRowModel>(filters);
            }

            // Sorting
            var sorted = subjects.Sort(command.SortDescriptors) as IQueryable<Subject>;

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize).ToList();

            // Data
            var data = paged.Select(x => CreateFeaturePermissionGridRowModel.Convert(x, selectedSubjects));

            return View(new GridModel<CreateFeaturePermissionGridRowModel>
            {
                Data = data,
                Total = total
            });
        }

        [HttpPost]
        public ActionResult SubmitSubjects(Dictionary<long, int> selection, long featureId)
        {
            var selectedSubjects = new Dictionary<long, int>();

            if (Session["SelectedSubjects"] != null)
                selectedSubjects = Session["SelectedSubjects"] as Dictionary<long, int>;

            var featureManager = new FeatureManager();
            var featurePermissionManager = new FeaturePermissionManager();

            var feature = featureManager.FindById(featureId);

            foreach (var featurePermission in feature.Permissions)
            {
                featurePermissionManager.Delete(featurePermission);
            }

            foreach (var subject in selectedSubjects)
            {
                featurePermissionManager.Create(subject.Key, featureId, (PermissionType)subject.Value);
            }

            Session["SelectedSubjects"] = null;

            return Json(new { success = true });
        }
    }
}