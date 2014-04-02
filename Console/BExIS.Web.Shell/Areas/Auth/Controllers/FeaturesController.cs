using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.Auth.Models;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;

namespace BExIS.Web.Shell.Areas.Auth.Controllers
{
    public class FeaturesController : Controller
    {
        #region Tree View
       
        public ActionResult Features()
        {
            FeatureManager featureManager = new FeatureManager();

            List<FeatureModel> features = new List<FeatureModel>();

            IQueryable<Feature> roots = featureManager.GetRoots();
            roots.ToList().ForEach(f => features.Add(FeatureModel.Convert(f)));

            return View(features.AsEnumerable<FeatureModel>());
        }

        #endregion


        #region Grid - FeaturePermissions

        public ActionResult FeaturePermissions(long id)
        {
            ViewData["FeatureID"] = id;

            FeatureManager featureManager = new FeatureManager();

            if (featureManager.ExistsFeatureId(id))
            {
                return PartialView("_FeaturePermissionsPartial");
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("Window_Details", "The feature does not exist!"));
            }
        }

        [GridAction]
        public ActionResult FeaturePermissions_Select(long id)
        {
            FeatureManager featureManager = new FeatureManager();

            // DATA
            Feature feature = featureManager.GetFeatureById(id);

            List<FeaturePermissionModel> featurePermissions = new List<FeaturePermissionModel>();

            if (feature != null)
            {
                PermissionManager permissionManager = new PermissionManager();
                SubjectManager subjectManager = new SubjectManager();

                IQueryable<Subject> data = subjectManager.GetAllSubjects();

                data.ToList().ForEach(s => featurePermissions.Add(FeaturePermissionModel.Convert(feature, s, permissionManager.GetFeaturePermissionType(feature.Id, s.Id))));
            }

            return View(new GridModel<FeaturePermissionModel> { Data = featurePermissions });
        }



        public void SetFeaturePermission(long subjectId, long featureId, int value)
        {
            PermissionManager permissionManager = new PermissionManager();

            if (value == 2)
            {
                permissionManager.DeleteFeaturePermissionByFeatureAndSubject(featureId, subjectId);
            }
            else
            {
                FeaturePermission featurePermission = permissionManager.GetFeaturePermissionByFeatureAndSubject(featureId, subjectId);

                if(featurePermission != null)
                {
                    featurePermission.PermissionType = (PermissionType)value;
                    permissionManager.UpdateFeaturePermission(featurePermission);
                }
                else
                {
                    FeaturePermissionCreateStatus status;
                    permissionManager.CreateFeaturePermission(featureId, subjectId, (PermissionType)value, out status);
                }
            }

            SubjectManager subjectManager = new SubjectManager();
        }

        #endregion


        #region Validation

        public JsonResult ValidateFeatureName(string featureName, long id = 0)
        {
            FeatureManager featureManager = new FeatureManager();

            Feature feature = featureManager.GetFeatureByName(featureName);

            if (feature == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (feature.Id == id)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string error = String.Format(CultureInfo.InvariantCulture, "The feature name already exists.", featureName);

                    return Json(error, JsonRequestBehavior.AllowGet);
                }
            }
        }

        private static string ErrorCodeToErrorMessage(FeatureCreateStatus status)
        {
            switch (status)
            {
                case FeatureCreateStatus.DuplicateFeatureName:
                    return "The feature name already exists.";

                case FeatureCreateStatus.InvalidFeatureName:
                    return "The feature name is not valid.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }


        private static string ErrorCodeToErrorKey(FeatureCreateStatus status)
        {
            switch (status)
            {
                case FeatureCreateStatus.DuplicateFeatureName:
                    return "FeatureName";

                case FeatureCreateStatus.InvalidFeatureName:
                    return "FeatureName";

                default:
                    return "";
            }
        }

        #endregion
    }
}
