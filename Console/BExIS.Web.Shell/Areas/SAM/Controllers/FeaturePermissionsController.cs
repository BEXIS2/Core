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
            var featurePermissionManager = new FeaturePermissionManager();
            var subjects = featurePermissionManager.FeaturePermissions.Where(x => x.Feature.Id == featureId)
                    .Select(x => new KeyValuePair<long, int>(x.Subject.Id, (int)x.PermissionType));

            subjects.ToDictionary(x => x.Key, x => x.Value);

            Session["SelectedSubjects"] = subjects.ToDictionary(x => x.Key, x => x.Value);

            return PartialView("_Permissions", featureId);
        }

        public void updateSelection(Dictionary<long, int> selection)
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
                else
                {
                    selectedSubjects.Remove(item.Key);
                }
            }

            Session["SelectedSubjects"] = selectedSubjects;
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult FeaturePermissions_Select(Dictionary<long, int> selection, long featureId, GridCommand command)
        {
            updateSelection(selection);

            var subjectManager = new SubjectManager();

            // Source + Transformation - Data
            var subjects = subjectManager.Subjects;
            var total = subjects.Count();

            // Filtering
            var filters = command.FilterDescriptors as List<FilterDescriptor>;

            if (filters != null)
            {
                subjects = subjects.FilterBy<Subject, FeaturePermissionGridRowModel>(filters);
            }

            // Sorting
            var sorted = subjects.Sort(command.SortDescriptors) as IQueryable<Subject>;

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize).ToList();

            // Data
            var data = paged.Select(x => FeaturePermissionGridRowModel.Convert(x, Session["SelectedSubjects"] as Dictionary<long, int>));

            return View(new GridModel<FeaturePermissionGridRowModel>
            {
                Data = data,
                Total = total
            });
        }

        [HttpPost]
        public ActionResult FeaturePermissions_Save(Dictionary<long, int> selection, long featureId)
        {
            updateSelection(selection);

            var featureManager = new FeatureManager();
            var featurePermissionManager = new FeaturePermissionManager();

            var feature = featureManager.FindById(featureId);

            var featurePermissions = feature.Permissions.Select(p => p.Id).ToArray();
            foreach (var id in featurePermissions)
            {
                featurePermissionManager.Delete(featurePermissionManager.FindById(id));
            }

            foreach (var subject in Session["SelectedSubjects"] as Dictionary<long, int>)
            {
                featurePermissionManager.Create(subject.Key, featureId, (PermissionType)subject.Value);
            }

            Session["SelectedSubjects"] = null;

            return Json(new { success = true });
        }
    }
}