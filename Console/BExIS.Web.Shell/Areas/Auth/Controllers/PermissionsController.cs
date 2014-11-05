using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
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
    public class PermissionsController : Controller
    {
        //#region Features

        //#region Tree View

        //public ActionResult Features()
        //{
        //    FeatureManager featureManager = new FeatureManager();

        //    List<FeatureModel> features = new List<FeatureModel>();

        //    IQueryable<Feature> roots = featureManager.GetRoots();
        //    roots.ToList().ForEach(f => features.Add(FeatureModel.Convert(f)));

        //    return View(features.AsEnumerable<FeatureModel>());
        //}

        //#endregion

        //#region Grid - FeaturePermissions

        //public ActionResult FeaturePermissions(long id)
        //{
        //    ViewData["FeatureID"] = id;

        //    FeatureManager featureManager = new FeatureManager();

        //    if (featureManager.ExistsFeatureId(id))
        //    {
        //        return PartialView("_FeaturePermissionsPartial");
        //    }
        //    else
        //    {
        //        return PartialView("_InfoPartial", new InfoModel("Window_Details", "The feature does not exist!"));
        //    }
        //}

        //[GridAction]
        //public ActionResult FeaturePermissions_Select(long id)
        //{
        //    FeatureManager featureManager = new FeatureManager();

        //    // DATA
        //    Feature feature = featureManager.GetFeatureById(id);

        //    List<FeaturePermissionModel> featurePermissions = new List<FeaturePermissionModel>();

        //    if (feature != null)
        //    {
        //        PermissionManager permissionManager = new PermissionManager();
        //        SubjectManager subjectManager = new SubjectManager();

        //        IQueryable<Subject> data = subjectManager.GetAllSubjects();

        //        //data.ToList().ForEach(s => featurePermissions.Add(FeaturePermissionModel.Convert(feature, s, permissionManager.ExistsFeaturePermission(feature.Id, s.Id), permissionManager.HasSubjectFeatureAccess(feature.Id, s.Id))));
        //    }

        //    return View(new GridModel<FeaturePermissionModel> { Data = featurePermissions });
        //}

        //public FeaturePermission CreateFeaturePermission(long subjectId, long featureId)
        //{
        //    PermissionManager permissionManager = new PermissionManager();

        //    return permissionManager.CreateFeaturePermission(subjectId, featureId);
        //}

        //public bool DeleteFeaturePermission(long subjectId, long featureId)
        //{
        //    PermissionManager permissionManager = new PermissionManager();

        //    return permissionManager.DeleteFeaturePermission(subjectId, featureId);
        //}

        //#endregion

        //#region Validation

        //public JsonResult ValidateFeatureName(string featureName, long id = 0)
        //{
        //    FeatureManager featureManager = new FeatureManager();

        //    Feature feature = featureManager.GetFeatureByName(featureName);

        //    if (feature == null)
        //    {
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        if (feature.Id == id)
        //        {
        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            string error = String.Format(CultureInfo.InvariantCulture, "The feature name already exists.", featureName);

        //            return Json(error, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        //private static string ErrorCodeToErrorMessage(FeatureCreateStatus status)
        //{
        //    switch (status)
        //    {
        //        case FeatureCreateStatus.DuplicateFeatureName:
        //            return "The feature name already exists.";

        //        case FeatureCreateStatus.InvalidFeatureName:
        //            return "The feature name is not valid.";

        //        default:
        //            return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
        //    }
        //}

        //private static string ErrorCodeToErrorKey(FeatureCreateStatus status)
        //{
        //    switch (status)
        //    {
        //        case FeatureCreateStatus.DuplicateFeatureName:
        //            return "FeatureName";

        //        case FeatureCreateStatus.InvalidFeatureName:
        //            return "FeatureName";

        //        default:
        //            return "";
        //    }
        //}

        //#endregion

        //#endregion

        //#region Data

        //#region Grid - Data

        //public ActionResult Data()
        //{
        //    return View(new DataModel());
        //}

        //public ActionResult Entities(string entityId)
        //{
        //    return View();
        //}

        //public ActionResult Datasets()
        //{
        //    return PartialView("_DatasetsPartial");
        //}

        //[GridAction]
        //public ActionResult Datasets_Select()
        //{
        //    DatasetManager datasetManager = new DatasetManager();

        //    // DATA
        //    IQueryable<Dataset> data = datasetManager.DatasetRepo.Query();

        //    List<DatasetModel> datasets = new List<DatasetModel>();
        //    data.ToList().ForEach(d => datasets.Add(DatasetModel.Convert(d)));

        //    return View(new GridModel<DatasetModel> { Data = datasets });
        //}

        //public ActionResult Subjects(long dataId, string entityId)
        //{
        //    ViewData["DataId"] = dataId;
        //    ViewData["EntityId"] = entityId;

        //    return PartialView("_SubjectsPartial");
        //}

        //[GridAction]
        //public ActionResult Subjects_Select(long dataId, long entityId)
        //{
        //    PermissionManager permissionManager = new PermissionManager();
        //    SubjectManager subjectManager = new SubjectManager();

        //    List<DataSubjectModel> subjects = new List<DataSubjectModel>();

        //    IQueryable<Subject> data = subjectManager.GetAllSubjects();
        //    data.ToList().ForEach(s => subjects.Add(DataSubjectModel.Convert(dataId, entityId, s, permissionManager.ExistsDataPermission(dataId, entityId, s.Id, RightType.Read), permissionManager.ExistsDataPermission(dataId, entityId, s.Id, RightType.Update), permissionManager.ExistsDataPermission(dataId, entityId, s.Id, RightType.Delete))));

        //    return View(new GridModel<DataSubjectModel> { Data = subjects });
        //}

        //public DataPermission CreateDataPermission(long subjectId, long entityId, long dataId, int rightType)
        //{
        //    PermissionManager permissionManager = new PermissionManager();

        //    return permissionManager.CreateDataPermission(subjectId, entityId, dataId, (RightType)rightType);
        //}

        //public bool DeleteDataPermission(long subjectId, long entityId, long dataId, int rightType)
        //{
        //    PermissionManager permissionManager = new PermissionManager();

        //    return false; // permissionManager.DeleteDataPermission(subjectId, entityId, dataId, (RightType)rightType);
        //}

        //#endregion

        //#region Validation

        //#endregion

        //#endregion
    }
}
