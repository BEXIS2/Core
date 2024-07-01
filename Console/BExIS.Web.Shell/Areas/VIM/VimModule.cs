using BExIS.Modules.Vim.UI.Helper;
using System;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Vim.UI
{
    public class VimModule : ModuleBase
    {
        public VimModule() : base("vim")
        {
            LoggerFactory.GetFileLogger().LogCustom("...ctor of vim...");
        }

        public override void Install()
        {
            LoggerFactory.GetFileLogger().LogCustom("...start install of vim...");

            try
            {
                base.Install();

                using (var generator = new VIMSeedDataGenerator())
                {
                    generator.GenerateSeedData();
                }
            }
            catch (Exception e)
            {
                LoggerFactory.GetFileLogger().LogCustom(e.Message);
                LoggerFactory.GetFileLogger().LogCustom(e.StackTrace);
            }

            LoggerFactory.GetFileLogger().LogCustom("...end install of vim...");
        }
    }
}