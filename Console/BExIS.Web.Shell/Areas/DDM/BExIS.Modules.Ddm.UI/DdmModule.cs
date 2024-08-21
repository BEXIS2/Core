using BExIS.Modules.Ddm.UI.Helpers;
using System;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI
{
    public class DdmModule : ModuleBase
    {
        public DdmModule() : base("ddm")
        {
            LoggerFactory.GetFileLogger().LogCustom("...ctor of ddm...");
        }

        public override void Install()
        {
            LoggerFactory.GetFileLogger().LogCustom("...start install of ddm...");
            try
            {
                base.Install();

                using (var generator = new DdmSeedDataGenerator())
                {
                    generator.GenerateSeedData();
                }
            }
            catch (Exception e)
            {
                LoggerFactory.GetFileLogger().LogCustom(e.Message);
                LoggerFactory.GetFileLogger().LogCustom(e.StackTrace);
            }

            LoggerFactory.GetFileLogger().LogCustom("...end install of ddm...");
        }

        public override void Start()
        {
            base.Start();
            Vaiona.IoC.IoCFactory.Container.RegisterHeirarchical(typeof(BExIS.Ddm.Api.ISearchProvider), typeof(BExIS.Ddm.Providers.LuceneProvider.SearchProvider));
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }

        public override void Uninstall()
        {
            base.Uninstall();
        }
    }
}