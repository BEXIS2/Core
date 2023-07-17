using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using System.Linq;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class SubmitHook : Hook
    {
        public SubmitHook()
        {
            Start = "/dcm/submit/start";
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
                EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "detailes", HookMode.edit, id);

                // check if data strutcure exist
                //using (var datasetManager = new DatasetManager())
                //{
                //    var dataset = datasetManager.GetDataset(id);
                //    if (dataset == null && dataset.DataStructure == null) { Status = HookStatus.Disabled; return; }
                //}

                //// check if file not exist
                //if (cache.Files == null || cache.Files.Any() == false) { Status = HookStatus.Inactive; return; }

                //// if file reader information exist
                //if (cache.ExcelFileReaderInfo == null && cache.AsciiFileReaderInfo == null) { Status = HookStatus.Inactive; return; }

                //// if file is not valid
                //if (cache.IsDataValid == false) { Status = HookStatus.Inactive; return; }

                // generate Validation hash - compare with stored

                //if everthing exist
                Status = HookStatus.Open;
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
            else Status = HookStatus.Inactive;
        }

        /// <summary>
        /// Update cache means means if the submit finsihed sucessfully
        /// the cache file should go to the version folder and clean up the temp folder for the
        /// next round
        /// </summary>
        /// <returns></returns>
        public override bool UpdateCache(params object[] arguments)
        {
            // load cache

            // Update cache

            // copy cache the real place

            return false;
        }
    }
}