using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class MetadataEditHook : Hook
    {
        public MetadataEditHook()
        {
            Start = "dcm/form/start";
        }

        public override void Check(long datasetId, string username)
        {
            // check status
            checkStatus(datasetId, username);

        }

        private void checkStatus(long datasetId, string username)
        {
            // check if the user has access rights to the entrypoint - set in Start
            bool hasAccess = hasUserAccessRights(username);
            // user rights
            bool hasRights = hasUserEntityRights(datasetId,username,RightType.Write);

            if(hasAccess == false || hasRights == false) Status = HookStatus.AccessDenied;
        }

    }
}