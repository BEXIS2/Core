using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dim.UI.Hooks
{
    public class PublishHook : Hook
    {
        public PublishHook()
        {
            Start = "/dim/publish/start";
        }

        public override void Check(long id, string username)
        {
            // check status
            checkStatus(id, username);

        }

        private void checkStatus(long id, string username)
        {

            Status = HookStatus.Open;

            // check if the user has access rights to the entrypoint - set in Start
            bool hasAccess = hasUserAccessRights(username);

            // user rights to the dataset
            bool hasRights = hasUserEntityRights(id, username,RightType.Write);

            // if one fail then access is denied
            if(hasAccess == false || hasRights == false) Status = HookStatus.AccessDenied;

            using (var datasetManager = new DatasetManager())
            {
                var dataset = datasetManager.GetDataset(id);
                if (dataset.Status != Dlm.Entities.Data.DatasetStatus.CheckedIn) Status = HookStatus.Disabled;
            }


        }

    }
}