using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class UserPermissionsController : Controller
    {
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Grant)]
        public ActionResult Start(long id, int version = 0)
        {
            using (var entityManager = new EntityManager())
            {
                return RedirectToAction("SubjectsView", "UserPermissions", new { EntityId = entityManager.FindByName("Dataset").Id, InstanceId = id });
            }
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Grant)]
        public ActionResult StartView(long id, int version = 0)
        {
            using (var entityManager = new EntityManager())
            {
                return RedirectToAction("SubjectsPartialView", "UserPermissions", new { EntityId = entityManager.FindByName("Dataset").Id, InstanceId = id });
            }
        }

        public async Task AddRightToEntityPermission(long subjectId, long entityId, long instanceId, int rightType)
        {
            using (var entityPermissionManager = new EntityPermissionManager())
            {
                var entityPermission = await entityPermissionManager.FindAsync(subjectId, entityId, instanceId);

                if (entityPermission == null)
                {
                    await entityPermissionManager.CreateAsync(subjectId, entityId, instanceId, rightType);
                }
                else
                {
                    if ((entityPermission.Rights & rightType) != 0) return;
                    entityPermission.Rights += rightType;
                    await entityPermissionManager.UpdateAsync(entityPermission);
                }
            }
        }

        public async Task RemoveRightFromEntityPermission(long subjectId, long entityId, long instanceId, int rightType)
        {
            using (var entityPermissionManager = new EntityPermissionManager())
            {
                var entityPermission = await entityPermissionManager.FindAsync(subjectId, entityId, instanceId);

                if (entityPermission == null) return;

                if (entityPermission.Rights == rightType)
                {
                    await entityPermissionManager.DeleteAsync(entityPermission);
                }
                else
                {
                    if ((entityPermission.Rights & rightType) == 0) return;
                    entityPermission.Rights -= rightType;
                    await entityPermissionManager.UpdateAsync(entityPermission);
                }
            }
        }

        public ActionResult SubjectsPartialView(long entityId, long instanceId)
        {
            return PartialView("_Subjects", new EntityInstanceModel() { EntityId = entityId, InstanceId = instanceId });
        }

        public ActionResult SubjectsView(long entityId, long instanceId)
        {
            return View("_Subjects", new EntityInstanceModel() { EntityId = entityId, InstanceId = instanceId});
        }

        [GridAction]
        public ActionResult Subjects_Select(long entityId, long instanceId)
        {
            var subjectManager = new SubjectManager();
            var entityPermissionManager = new EntityPermissionManager();

            try
            {
                var subjects = new List<EntityPermissionGridRowModel>();
                using (var partyManager = new PartyManager())

                    foreach (var subject in subjectManager.Subjects)
                    {
                        var rights = entityPermissionManager.GetRightsAsync(subject.Id, entityId, instanceId).Result;
                        var effectiveRights = entityPermissionManager.GetEffectiveRightsAsync(subject.Id, entityId, instanceId).Result;

                        subjects.Add(EntityPermissionGridRowModel.Convert(subject, rights, effectiveRights));
                    }

                return View(new GridModel<EntityPermissionGridRowModel> { Data = subjects });
            }
            finally
            {
                subjectManager.Dispose();
                entityPermissionManager.Dispose();
            }
        }
    }
}