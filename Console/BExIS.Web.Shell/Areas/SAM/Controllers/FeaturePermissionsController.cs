using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Authorization;
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
    public class FeaturePermissionsController : Controller
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="featureId"></param>
        public void AddFeatureToPublic(long featureId)
        {
            var featurePermissionManager = new FeaturePermissionManager();

            try
            {
                if (!featurePermissionManager.Exists(null, featureId))
                {
                    featurePermissionManager.Create(null, featureId, PermissionType.Grant);
                }
            }
            finally
            {
                featurePermissionManager.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="featureId"></param>
        /// <param name="permissionType"></param>
        public void CreateOrUpdateFeaturePermission(long? subjectId, long featureId, int permissionType)
        {
            var featurePermissionManager = new FeaturePermissionManager();

            try
            {
                var featurePermission = featurePermissionManager.Find(subjectId, featureId);

                if (featurePermission != null)
                {
                    featurePermission.PermissionType = (PermissionType)permissionType;
                    featurePermissionManager.Update(featurePermission);
                }
                else
                {
                    featurePermissionManager.Create(subjectId, featureId, (PermissionType)permissionType);
                }
            }
            finally
            {
                featurePermissionManager.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="featureId"></param>
        /// <param name="permissionType"></param>
        public void DeleteFeaturePermission(long subjectId, long featureId)
        {
            var featurePermissionManager = new FeaturePermissionManager();

            try
            {
                featurePermissionManager.Delete(subjectId, featureId);
            }
            finally
            {
                featurePermissionManager.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var featureManager = new FeatureManager();

            try
            {
                ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Features", this.Session.GetTenant());

                var features = new List<FeatureTreeViewModel>();

                var roots = featureManager.FindRoots();
                roots.ToList().ForEach(f => features.Add(FeatureTreeViewModel.Convert(f)));

                return View(features.AsEnumerable());
            }
            finally
            {
                featureManager.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="featureId"></param>
        public void RemoveFeatureFromPublic(long featureId)
        {
            var featurePermissionManager = new FeaturePermissionManager();

            try
            {
                if (featurePermissionManager.Exists(null, featureId))
                {
                    featurePermissionManager.Delete(null, featureId);
                }
            }
            finally
            {
                featurePermissionManager.Dispose(); ;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="featureId"></param>
        /// <returns></returns>
        public ActionResult Subjects(long featureId)
        {
            return PartialView("_Subjects", featureId);
        }

        [GridAction]
        public ActionResult Subjects_Select(long featureId)
        {
            var featureManager = new FeatureManager();
            var featurePermissionManager = new FeaturePermissionManager();
            var subjectManager = new SubjectManager();

            try
            {
                var feature = featureManager.FindById(featureId);

                var featurePermissions = new List<FeaturePermissionGridRowModel>();

                if (feature == null)
                    return View(new GridModel<FeaturePermissionGridRowModel> { Data = featurePermissions });
                var data = subjectManager.Subjects;

                data.ToList()
                    .ForEach(
                        s =>
                            featurePermissions.Add(FeaturePermissionGridRowModel.Convert(s, feature,
                                featurePermissionManager.GetPermissionType(s.Id, feature.Id),
                                featurePermissionManager.HasAccess(s.Id, feature.Id))));

                return View(new GridModel<FeaturePermissionGridRowModel> { Data = featurePermissions });
            }
            finally
            {
                featureManager.Dispose();
                featurePermissionManager.Dispose();
                subjectManager.Dispose();
            }
        }
    }
}