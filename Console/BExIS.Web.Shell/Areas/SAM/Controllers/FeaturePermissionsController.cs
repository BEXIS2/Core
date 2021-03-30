using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.Utils.NH.Querying;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class FeaturePermissionsController : BaseController
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="featureId"></param>
        public void AddFeatureToPublic(long featureId)
        {
            using (var featurePermissionManager = new FeaturePermissionManager())
            {
                if (!featurePermissionManager.Exists(null, featureId))
                {
                    featurePermissionManager.Create(null, featureId, PermissionType.Grant);
                }
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
            using (var featurePermissionManager = new FeaturePermissionManager())
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

                var features = featureManager.Features.Select(f => FeatureTreeViewModel.Convert(f, f.Permissions.Any(p => p.Subject == null), f.Parent.Id)).ToList();

                foreach (var feature in features)
                {
                    feature.Children = features.Where(f => f.ParentId == feature.Id).ToList();
                }

                return View(features.Where(f => f.ParentId == null).AsEnumerable());
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
            using (var featurePermissionManager = new FeaturePermissionManager())
            {
                if (featurePermissionManager.Exists(null, featureId))
                {
                    featurePermissionManager.Delete(null, featureId);
                }
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

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Subjects_Select(GridCommand command, long featureId)
        {
            using (var featurePermissionManager = new FeaturePermissionManager())
            using (var subjectManager = new SubjectManager())
            using (var featureManager = new FeatureManager())
            {
                var feature = featureManager.FindById(featureId);

                var featurePermissions = new List<FeaturePermissionGridRowModel>();

                if (feature == null)
                    return View(new GridModel<FeaturePermissionGridRowModel> { Data = featurePermissions });

                var subjects = new List<Subject>();
                int count = subjectManager.Subjects.Count();
                ViewData["subjectsGridTotal"] = count;
                if (command != null)// filter subjects based on grid filter settings
                {
                    FilterExpression filter = TelerikGridHelper.Convert(command.FilterDescriptors.ToList());
                    OrderByExpression orderBy = TelerikGridHelper.Convert(command.SortDescriptors.ToList());

                    subjects = subjectManager.GetSubjects(filter, orderBy, command.Page, command.PageSize, out count);
                }
                else
                {
                    subjects = subjectManager.Subjects.ToList();
                }

                //foreach (var subject in subjects)
                //{
                //    var rightType = featurePermissionManager.GetPermissionType(subject.Id, feature.Id);
                //    var hasAccess = featurePermissionManager.HasAccess(subject.Id, feature.Id);

                //    featurePermissions.Add(FeaturePermissionGridRowModel.Convert(subject, featureId, rightType, hasAccess));
                //}

                var subjectIds = subjects.Select(s => s.Id);
                var userPermissionDic = featurePermissionManager.GetPermissionType(subjectIds, feature.Id);
                var userHasAccessDic = featurePermissionManager.HasAccess(subjects, feature.Id);

                foreach (var item in userPermissionDic)
                {
                    var subject = subjects.Where(s => s.Id.Equals(item.Key)).FirstOrDefault();
                    var rightType = item.Value;
                    var hasAccess = userHasAccessDic[item.Key];

                    featurePermissions.Add(FeaturePermissionGridRowModel.Convert(subject, featureId, rightType, hasAccess));
                }

                return View(new GridModel<FeaturePermissionGridRowModel> { Data = featurePermissions, Total = count });
            }
        }
    }
}