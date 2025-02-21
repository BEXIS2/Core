using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using NHibernate.Util;
using System.Linq;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class DataEditHook : Hook
    {
        public DataEditHook()
        {
            Start = "/dcm/data/start";
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
            bool hasRights = hasUserEntityRights(id, username, RightType.Write);

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
                var version = datasetManager.GetDatasetLatestVersion(id);

                if (dataset != null && version != null)
                { 
                    if(dataset.DataStructure == null) // no structure 
                    {
                        //checi if content exist
                        if (!version.ContentDescriptors.Any() || !version.ContentDescriptors.Where(c => c.Name.Equals("unstructuredData")).Any()) //no content or files for unstructured data
                            // has data files
                                Status = HookStatus.Disabled;
                            else
                                Status = HookStatus.Open;
                    }
                    else // has structure but data?
                    {
                        //ToDo implelemnt the data view and set the status to open if data is available
                        var count = datasetManager.RowCount(id);
                        if(count >0) Status = HookStatus.Open;
                        else Status = HookStatus.Open;
                    }

                    // check dataset status
                    if (dataset.Status != Dlm.Entities.Data.DatasetStatus.CheckedIn) Status = HookStatus.Disabled;

                }
            }
        }
    }
}