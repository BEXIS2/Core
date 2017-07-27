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
                // ToDo: refactor the seed data generator
                // at BExIS.Modules.Ddm.UI.Helpers.DdmSeedDataGenerator.<>c__DisplayClass0_0.<GenerateSeedData>b__1(Feature f) in C:\Users\standard\Source\BExIS\Core\Repos\Code\Console\BExIS.Web.Shell\Areas\DDM\Helpers\DdmSeedDataGenerator.cs:line 25
                // at System.Linq.Enumerable.FirstOrDefault[TSource](IEnumerable`1 source, Func`2 predicate)
                // at BExIS.Modules.Ddm.UI.Helpers.DdmSeedDataGenerator.GenerateSeedData() in C: \Users\standard\Source\BExIS\Core\Repos\Code\Console\BExIS.Web.Shell\Areas\DDM\Helpers\DdmSeedDataGenerator.cs:line 25
                // at BExIS.Modules.Ddm.UI.DdmModule.Install() in C: \Users\standard\Source\BExIS\Core\Repos\Code\Console\BExIS.Web.Shell\Areas\DDM\DdmModule.cs:line 21
                //DdmSeedDataGenerator.GenerateSeedData();
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
