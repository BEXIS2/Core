using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.UI.Hooks;

namespace BExIS.Modules.Dim.UI.Hooks
{
    public class PublishViewHook : Hook
    {
        public PublishViewHook()
        {
            Start = "/dim/publish/startView";
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
            Status = HookStatus.Open;

            // check if the user has access rights to the entrypoint - set in Start
            bool hasAccess = hasUserAccessRights(username);

            // user rights to the dataset
            bool hasRights = hasUserEntityRights(id, username, RightType.Write);

            // if one fail then access is denied
            if (hasAccess == false || hasRights == false) Status = HookStatus.AccessDenied;

            using (var datasetManager = new DatasetManager())
            {
                var dataset = datasetManager.GetDataset(id);
                if (dataset.Status != Dlm.Entities.Data.DatasetStatus.CheckedIn) Status = HookStatus.Disabled;
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