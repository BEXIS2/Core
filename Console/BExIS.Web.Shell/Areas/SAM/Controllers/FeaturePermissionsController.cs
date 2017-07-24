using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public delegate bool IsFeatureInEveryoneGroupDelegate(long featureId);

    public class FeaturePermissionsController : Controller
    {
        public ActionResult Features()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Features", this.Session.GetTenant());

            var featureManager = new FeatureManager();

            var features = new List<FeatureTreeViewModel>();

            var roots = featureManager.FindRoots();
            roots.ToList().ForEach(f => features.Add(FeatureTreeViewModel.Convert(f, new IsFeatureInEveryoneGroupDelegate(IsFeatureInEveryoneGroup))));

            return PartialView("_Features", features.AsEnumerable());
        }

        public ActionResult Index()
        {
            return View();
        }

        public bool IsFeatureInEveryoneGroup(long featureId)
        {
            var featurePermissionManager = new FeaturePermissionManager();
            var subjectManager = new SubjectManager();

            return featurePermissionManager.Exists(0, featureId);
        }

        public bool SetFeaturePermission(long subjectId, long featureId, int value)
        {
            var featurePermissionManager = new FeaturePermissionManager();

            if (value == 2)
            {
                featurePermissionManager.Delete(subjectId, featureId);

                return true;
            }
            else
            {
                var featurePermission = featurePermissionManager.Find(featureId, subjectId);

                if (featurePermission != null)
                {
                    featurePermission.PermissionType = (PermissionType)value;
                    featurePermissionManager.Update(featurePermission);

                    return true;
                }
                else
                {
                    featurePermissionManager.Create(subjectId, featureId, (PermissionType)value);

                    return true;
                }
            }
        }

        public bool SetFeaturePublicity(long featureId, bool value)
        {
            var featureManager = new FeatureManager();
            var permissionManager = new PermissionManager();
            var subjectManager = new SubjectManager();

            var feature = featureManager.FindById(featureId);

            // ToDo
            //if (feature != null)
            //{
            //    if (value)
            //    {
            //        permissionManager.CreateFeaturePermission(subjectManager.GetGroupByName("everyone").Id, feature.Id);
            //    }
            //    else
            //    {
            //        permissionManager.DeleteFeaturePermission(subjectManager.GetGroupByName("everyone").Id, feature.Id);
            //    }

            //    return true;
            //}

            return false;
        }

        public ActionResult Subjects(long id)
        {
            ViewData["FeatureId"] = id;

            return PartialView("_Subjects");
        }

        [GridAction]
        public ActionResult Subjects_Select(long featureId)
        {
            var featureManager = new FeatureManager();

            // DATA
            Feature feature = featureManager.FindById(featureId);

            var featurePermissions = new List<FeaturePermissionGridRowModel>();

            if (feature != null)
            {
                var featurePermissionManager = new FeaturePermissionManager();
                var subjectManager = new SubjectManager();

                var data = subjectManager.Subjects;

                data.ToList().ForEach(s => featurePermissions.Add(FeaturePermissionGridRowModel.Convert(s, feature, featurePermissionManager.GetPermissionType(s.Id, feature.Id), featurePermissionManager.HasAccess(s.Id, feature.Id))));
            }

            return View(new GridModel<FeaturePermissionGridRowModel> { Data = featurePermissions });
        }
    }
}