using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.UI.Hooks;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class LinkViewHook : Hook
    {
        public LinkViewHook()
        {
            Start = "/dcm/entityreference/startview";
        }

        public override void Check(long id, string username)
        {
            // disable for extension entity
            checkEntity(id);

            if (Status != HookStatus.Disabled)
            {

                // check status
                checkStatus(id, username);

            }
        }

        private void checkStatus(long id, string username)
        {

             Status = HookStatus.Open;

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