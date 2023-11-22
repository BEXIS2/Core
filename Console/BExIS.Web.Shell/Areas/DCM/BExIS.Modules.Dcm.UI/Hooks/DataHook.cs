using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class DataHook : Hook
    {
        public DataHook()
        {
            Start = "/dcm/view/startData";
        }

        public override void Check(long id, string username)
        {
            // check status
            checkStatus(id, username);

            // if status is open then check if data is available
            if (Status == HookStatus.Open) checkDataStatus(id, username);
        }

        private void checkStatus(long id, string username)
        {
            // check if the user has access rights to the entrypoint - set in Start
            bool hasAccess = hasUserAccessRights(username);

            // user rights to the dataset
            bool hasRights = hasUserEntityRights(id, username, RightType.Read);

            // if one fail then access is denied
            if (hasAccess == false || hasRights == false) Status = HookStatus.AccessDenied;
            else Status = HookStatus.Open;
        }

        private void checkDataStatus(long id, string username)
        {
            // check if dataset has description
            using (var datasetManager = new DatasetManager())
            {
                var dataset = datasetManager.GetDataset(id);
                if (dataset == null && dataset.DataStructure == null) { Status = HookStatus.Disabled; return; }
                else Status = HookStatus.Open;

            }
        }

    }
}