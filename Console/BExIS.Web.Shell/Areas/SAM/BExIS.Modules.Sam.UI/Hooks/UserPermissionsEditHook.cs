using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Sam.UI.Hooks
{
    public class UserPermissionsEditHook : Hook
    {
        public UserPermissionsEditHook()
        {
            Start = "/sam/UserPermissions/start";
        }

        public override void Check(long id, string username)
        {
            // check status
            checkStatus(id, username);

            // disable for extension entity
            checkEntity(id);

        }

        private void checkStatus(long id, string username)
        {
            // check if the user has access rights to the entrypoint - set in Start
            bool hasAccess = hasUserAccessRights(username);

            // user rights to the dataset
            bool hasRights = hasUserEntityRights(id, username,RightType.Grant);

            // if one fail then access is denied
            if (hasAccess == false || hasRights == false)
            {
                Status = HookStatus.AccessDenied;
            }
            else
            {
                Status = HookStatus.Open;
            }

        }

        private void checkEntity(long id)
        {
            using (var datasetManager = new DatasetManager())
            using (var entityManager = new EntityManager())
            {
                var entity = entityManager.FindByName("extension"); // get entity
                if (entity != null)
                {
                    Status = datasetManager.GetDataset(id).EntityTemplate.EntityType.Id.Equals(entity.Id) ? HookStatus.Disabled : Status; // disable if entity type matches
                }
            }
        }

    }
}