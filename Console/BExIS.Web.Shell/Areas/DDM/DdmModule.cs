using BExIS.Modules.Ddm.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI
{
    public class DdmModule : ModuleBase
    {
        public DdmModule(): base("ddm")
        {
        }

        public override void Install()
        {
            base.Install();
            DdmSeedDataGenerator.GenerateSeedData();
        }
    }
}
