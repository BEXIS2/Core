using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using System.Linq;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class ValidationHook : Hook
    {
        public ValidationHook()
        {
            Start = "/dcm/validation/start";
        }

        public override void Check(long id, string username)
        {
            // check status
            checkStatus(id, username);

            // after the security check this hook needs to check
            // wheter incoming data, and datastructure exist
            if (Status != HookStatus.AccessDenied)
            {
                HookManager hookManager = new HookManager();
                EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);

                // check if data strutcure exist
                using (var datasetManager = new DatasetManager())
                {
                    var dataset = datasetManager.GetDataset(id);
                    if (dataset == null || dataset.DataStructure == null) { Status = HookStatus.Disabled; return; }
                }

                // check if file not exist
                if (cache.Files == null || cache.Files.Any() == false) { Status = HookStatus.Inactive; return; }

                // if file reader information exist
                // if (cache.ExcelFileReaderInfo == null && cache.AsciiFileReaderInfo == null) { Status = HookStatus.Inactive; return; }

                // if everthing exist
                Status = HookStatus.Open;
            }

            checkAvailablity(id);
        }

        private void checkStatus(long id, string username)
        {
            // check if the user has access rights to the entrypoint - set in Start
            bool hasAccess = hasUserAccessRights(username);

            // user rights to the dataset
            bool hasRights = hasUserEntityRights(id, username, RightType.Write);

            // if one fail then access is denied
            if (hasAccess == false || hasRights == false) Status = HookStatus.AccessDenied;
            else Status = HookStatus.Inactive;
        }

        /// <summary>
        /// after a validation run, the result should be stored
        /// the result is
        /// 1. data is valid
        /// 2. list of Errors
        /// </summary>
        /// <returns></returns>
        public override bool UpdateCache(params object[] arguments)
        {
            // load cache

            // Update cache

            return false;
        }

        private void checkAvailablity(long id)
        {
            // check if subject is checked in
            // only check if status is open
            if (Status == HookStatus.Open)
                using (var datasetManager = new DatasetManager())
                    Status = datasetManager.IsDatasetCheckedIn(id) == true ? HookStatus.Open : HookStatus.Waiting;
        }
    }
}