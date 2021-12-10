using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class SubmitHook : Hook
    {
        public SubmitHook()
        {
            Start = "dcm/submit/start";
        }

        public override void Check(long id, string username)
        {
            // check status
            checkStatus(id, username);

            // check - chache
            // if data is valid
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