using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class DataDescriptionHook : Hook
    {
        public DataDescriptionHook()
        {
            Start = "/dcm/datadescription/start";
        }

        public override void Check(long id, string username)
        {
            // check status
            checkPermissionStatus(id, username);

            // if status is open then check if data is available
            if(Status == HookStatus.Open) checkDataStatus(id, username);

        }

        private void checkPermissionStatus(long id, string username)
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
            using (var entityTemplateManager = new EntityTemplateManager())
            {
                var dataset = datasetManager.GetDataset(id);
                var template = entityTemplateManager.Repo.Get(dataset.EntityTemplate.Id);
                if (dataset == null || template == null || template.HasDatastructure == false) { Status = HookStatus.Disabled; return; }
                else Status = HookStatus.Open;

            }
        }
    }
}