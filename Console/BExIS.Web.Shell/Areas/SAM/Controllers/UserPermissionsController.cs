using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using System.Collections.Generic;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class UserPermissionsController : Controller
    {
        public void AddRightToEntityPermission(long subjectId, long entityId, long instanceId, int rightType)
        {
            var entityPermissionManager = new EntityPermissionManager();

            try
            {
                var entityPermission = entityPermissionManager.Find(subjectId, entityId, instanceId);

                if (entityPermission == null)
                {
                    entityPermissionManager.Create(subjectId, entityId, instanceId, rightType);
                }
                else
                {
                    if ((entityPermission.Rights & rightType) != 0) return;
                    entityPermission.Rights += rightType;
                    entityPermissionManager.Update(entityPermission);
                }
            }
            finally
            {
                entityPermissionManager.Dispose();
            }
        }

        public void RemoveRightFromEntityPermission(long subjectId, long entityId, long instanceId, int rightType)
        {
            var entityPermissionManager = new EntityPermissionManager();

            try
            {
                var entityPermission = entityPermissionManager.Find(subjectId, entityId, instanceId);

                if (entityPermission == null) return;

                if (entityPermission.Rights == rightType)
                {
                    entityPermissionManager.Delete(entityPermission);
                }
                else
                {
                    if ((entityPermission.Rights & rightType) == 0) return;
                    entityPermission.Rights -= rightType;
                    entityPermissionManager.Update(entityPermission);
                }
            }
            finally
            {
                entityPermissionManager.Dispose();
            }
        }

        public ActionResult Subjects(long entityId, long instanceId)
        {
            return PartialView("_Subjects", new EntityInstanceModel() { EntityId = entityId, InstanceId = instanceId });
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
                    var rights = entityPermissionManager.GetRights(subject.Id, entityId, instanceId);
                    var effectiveRights = entityPermissionManager.GetEffectiveRights(subject.Id, entityId, instanceId);

     
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
