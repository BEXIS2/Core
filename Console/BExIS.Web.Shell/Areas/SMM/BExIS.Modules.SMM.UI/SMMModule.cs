using BExIS.Modules.Smm.UI.Helpers;
using System;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Smm.UI
{
    public class SmmModule : ModuleBase
    {
        public SmmModule() : base("smm")
        {
            LoggerFactory.GetFileLogger().LogCustom("...ctor of smm...");
        }

        public override void Install()
        {
            LoggerFactory.GetFileLogger().LogCustom("...start install of smm...");
            try
            {
                base.Install();

                using (var generator = new SMMSeedDataGenerator())
                {
                    generator.GenerateSeedData();
                }
            }
            catch (Exception e)
            {
                LoggerFactory.GetFileLogger().LogCustom(e.Message);
                LoggerFactory.GetFileLogger().LogCustom(e.StackTrace);
            }

            LoggerFactory.GetFileLogger().LogCustom("...end install of smm...");
        }
    }
}