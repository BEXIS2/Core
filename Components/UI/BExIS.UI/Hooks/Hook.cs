﻿using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.UI.Hooks
{
    public abstract class Hook: IHook
    {

        /// <summary>
        /// the name of the hook
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// the Status selected from teh HookStatus Enum
        /// </summary>
        public HookStatus Status { get; set; }

        /// <summary>
        /// the mode of the hook
        /// </summary>
        public HookMode Mode { get; set; } 

        /// <summary>
        /// return the name of the Enitiy
        /// </summary>
        /// <returns></returns>
        public string Entity { get; set; }

        /// <summary>
        /// return the ModuleId of the Hook
        /// </summary>
        /// <returns></returns>
        public string Module { get; set; }

        /// <summary>
        /// return the place for the hook
        /// </summary>
        /// <returns></returns>
        public string Place { get; set; }

        /// <summary>
        /// Start action where the workflow behind begins
        /// </summary>
        /// <returns></returns>
        public string Start { get; set; }

        public virtual void Check(long id, string username)
        {
            throw new NotImplementedException();
        }

        protected bool hasUserAccessRights(string username)
        {
            using (FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager())
            {
                return featurePermissionManager.HasAccess<User>(username, Start.Split('/')[0], Start.Split('/')[1], "*");
            }
        }


        /// <summary>
        /// return true if user has edit rights
        /// </summary>
        /// <returns></returns>
        protected bool hasUserEntityRights(long entityId,string userName, RightType rightType)
        {
            #region security permissions and authorisations check

            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            {
                return entityPermissionManager.HasEffectiveRight(userName, typeof(Dataset), entityId, rightType);
            }

            #endregion security permissions and authorisations check
        }


        public Hook() {
            Name = "";
            Mode = HookMode.view;
            Entity = "";
            Module = "";
            Place = "";
            Start = "";
        }

}
}
