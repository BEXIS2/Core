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