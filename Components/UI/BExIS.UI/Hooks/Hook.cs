﻿using System;
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

        public virtual void Check()
        {
            // check status
            // set entrypoint

            throw new NotImplementedException();
        }

        public Hook() {
            Name = "";
            Mode = HookMode.view;
            Entity = "";
            Module = "";
            Place = "";
        }

}
}