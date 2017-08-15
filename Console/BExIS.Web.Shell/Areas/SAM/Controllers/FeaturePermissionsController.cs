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
    public class FeaturePermissionsController : Controller
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="featureId"></param>
        public void AddFeatureToPublic(long featureId)
        {
            var featureManager = new FeatureManager();
            var feature = featureManager.FindById(featureId);
            //feature.IsPublic = true;
            featureManager.Update(feature);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="featureId"></param>
        /// <param name="permissionType"></param>
        public void CreateOrUpdateFeaturePermission(long subjectId, long featureId, int permissionType)
        {
            var featurePermissionManager = new FeaturePermissionManager();
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="featureId"></param>
        /// <param name="permissionType"></param>
        public void DeleteFeaturePermission(long subjectId, long featureId)
        {
            var featurePermissionManager = new FeaturePermissionManager();
            featurePermissionManager.Delete(subjectId, featureId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Features", this.Session.GetTenant());

            var featureManager = new FeatureManager();

            var features = new List<FeatureTreeViewModel>();

            var roots = featureManager.FindRoots();
            roots.ToList().ForEach(f => features.Add(FeatureTreeViewModel.Convert(f)));

            return View(features.AsEnumerable());
        }

        public bool IsFeaturePublic(long featureId)
        {
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="featureId"></param>
        public void RemoveFeatureFromPublic(long featureId)
        {
            var featureManager = new FeatureManager();
            var feature = featureManager.FindById(featureId);
            //feature.IsPublic = false;
            featureManager.Update(feature);
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