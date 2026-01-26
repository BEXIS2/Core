using BExIS.Modules.Pum.UI.Helpers;
using System;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Pum.UI
{
    public class PumModule : ModuleBase
    {
        public PumModule() : base("pum")
        {
            LoggerFactory.GetFileLogger().LogCustom("...ctor of pum...");
        }

        public override void Install()
        {
            LoggerFactory.GetFileLogger().LogCustom("...start install of pum...");
            try
            {
                base.Install();

                using (var generator = new PUMSeedDataGenerator())
                {
                    generator.GenerateSeedData();
                }
            }
            catch (Exception e)
            {
                LoggerFactory.GetFileLogger().LogCustom(e.Message);
                LoggerFactory.GetFileLogger().LogCustom(e.StackTrace);
            }

            LoggerFactory.GetFileLogger().LogCustom("...end install of pum...");
        }
    }
}