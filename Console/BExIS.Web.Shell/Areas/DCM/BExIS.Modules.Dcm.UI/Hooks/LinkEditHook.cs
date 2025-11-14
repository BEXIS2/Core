using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.UI.Hooks;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class LinkEditHook : Hook
    {
        public LinkEditHook()
        {
            Start = "/dcm/entityreference/start";
        }

        public override void Check(long id, string username)
        {
            // disable for extension entity
            checkEntity(id);

            if (Status != HookStatus.Disabled)
            {

                // check status
                checkStatus(id, username);

                // check if subject is checked in
                // only check if status is open
                checkAvailablity(id);
            }
        }

        private void checkStatus(long id, string username)
        {
            // check if the user has access rights to the entrypoint - set in Start
            bool hasAccess = hasUserAccessRights(username);

            // user rights to the dataset
            bool hasRights = hasUserEntityRights(id, username, RightType.Write);

            // if one fail then access is denied
            if (hasAccess == false || hasRights == false) Status = HookStatus.AccessDenied;
            else Status = HookStatus.Open;
        }

        private void checkAvailablity(long id)
        {
            // check if subject is checked in
            // only check if status is open
            if (Status == HookStatus.Open)
                using (var datasetManager = new DatasetManager())
                    Status = datasetManager.IsDatasetCheckedIn(id) == true ? HookStatus.Open : HookStatus.Waiting;
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