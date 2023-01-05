using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Sam.UI.Hooks
{
    public class UserPermissionsHook : Hook
    {
        public UserPermissionsHook()
        {
            Start = "sam/UserPermissions/start";
        }

        public override void Check(long id, string username)
        {
            // check status
            checkStatus(id, username);

        }

        private void checkStatus(long id, string username)
        {
            // check if the user has access rights to the entrypoint - set in Start
            bool hasAccess = hasUserAccessRights(username);

            // user rights to the dataset
            bool hasRights = hasUserEntityRights(id, username,RightType.Grant);

            // if one fail then access is denied
            if (hasAccess == false || hasRights == false)
            {
                Status = HookStatus.AccessDenied;
            }
            else
            {
                Status = HookStatus.Open;
            }

        }

    }
}