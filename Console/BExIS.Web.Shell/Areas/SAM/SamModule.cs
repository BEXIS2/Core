using BExIS.Modules.Sam.UI.Helpers;
using System;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Sam.UI
{
    public class SamModule : ModuleBase
    {
        public SamModule() : base("sam")
        {
            LoggerFactory.GetFileLogger().LogCustom("...ctor of sam...");
        }

        public override void Install()
        {
            LoggerFactory.GetFileLogger().LogCustom("...start install of sam...");
            try
            {
                base.Install();

                using (var generator = new SamSeedDataGenerator())
                {
                    generator.GenerateSeedData();
                }
            }
            catch (Exception e)
            {
                LoggerFactory.GetFileLogger().LogCustom(e.Message);
                LoggerFactory.GetFileLogger().LogCustom(e.StackTrace);
            }

            LoggerFactory.GetFileLogger().LogCustom("...end install of sam...");
        }
    }
}