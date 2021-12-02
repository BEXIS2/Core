using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class ValidationHook : Hook
    {
        public ValidationHook()
        {
            Start = "dcm/validation/start";
        }

        public override void Check(long id, string username)
        {
            // check status
            checkStatus(id, username);

            // after the security check this hook needs to check 
            // wheter incoming data, and datastructure exist
            if (Status == HookStatus.AccessDenied)
            {
                // check if file exist

                // check if data strutcure exist

            }
        }

        private void checkStatus(long id, string username)
        {
            // check if the user has access rights to the entrypoint - set in Start
            bool hasAccess = hasUserAccessRights(username);

            // user rights to the dataset
            bool hasRights = hasUserEntityRights(id, username,RightType.Write);

            // if one fail then access is denied
            if(hasAccess == false || hasRights == false) Status = HookStatus.AccessDenied;
            else Status = HookStatus.Inactive;
        }

    }
}