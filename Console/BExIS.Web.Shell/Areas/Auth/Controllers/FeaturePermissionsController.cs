using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.Auth.Models;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.Auth.Controllers
{
    public class FeaturePermissionsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Features()
        {
            FeatureManager featureManager = new FeatureManager();

            List<FeatureTreeViewModel> features = new List<FeatureTreeViewModel>();

            IQueryable<Feature> roots = featureManager.GetRoots();
            roots.ToList().ForEach(f => features.Add(FeatureTreeViewModel.Convert(f)));

            return View(features.AsEnumerable<FeatureTreeViewModel>());
        }

        public ActionResult Subjects(long id)
        {
            ViewData["FeatureId"] = id;

            FeatureManager featureManager = new FeatureManager();

            return PartialView("_SubjectsPartial");
        }

        [GridAction]
        public ActionResult Subjects_Select(long id)
        {
            FeatureManager featureManager = new FeatureManager();

            // DATA
            Feature feature = featureManager.GetFeatureById(id);

            List<FeaturePermissionGridRowModel> featurePermissions = new List<FeaturePermissionGridRowModel>();

            if (feature != null)
            {
                PermissionManager permissionManager = new PermissionManager();
                SubjectManager subjectManager = new SubjectManager();

                IQueryable<Subject> data = subjectManager.GetAllSubjects();

                data.ToList().ForEach(s => featurePermissions.Add(FeaturePermissionGridRowModel.Convert(s, feature, permissionManager.GetFeaturePermissionType(s.Id, feature.Id), permissionManager.HasSubjectFeatureAccess(s.Id, feature.Id))));
            }

            return View(new GridModel<FeaturePermissionGridRowModel> { Data = featurePermissions });
        }

        public bool SetFeaturePermission(long subjectId, long featureId, int value)
        {
            PermissionManager permissionManager = new PermissionManager();

            if (value == 2)
            {
                permissionManager.DeleteFeaturePermission(subjectId, featureId);

                return true;
            }
            else
            {
                FeaturePermission featurePermission = permissionManager.GetFeaturePermission(subjectId, featureId);

                if (featurePermission != null)
                {
                    featurePermission.PermissionType = (PermissionType)value;
                    permissionManager.UpdateFeaturePermission(featurePermission);

                    return true;
                }
                else
                {
                    permissionManager.CreateFeaturePermission(subjectId, featureId, (PermissionType)value);

                    return true;
                }
            }
        }
    }
}
