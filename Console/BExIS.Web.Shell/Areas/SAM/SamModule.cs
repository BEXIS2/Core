using BExIS.Modules.Sam.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Sam.UI
{
    public class SamModule : ModuleBase
    {
        public SamModule(): base("sam")
        {
        }

        public override void Install()
        {
            base.Install();
            SamSeedDataGenerator.GenerateSeedData();
        }
    }
}
