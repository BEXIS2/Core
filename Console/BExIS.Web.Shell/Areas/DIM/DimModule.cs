using BExIS.Modules.Dim.UI.Helpers;
using System;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dim.UI
{
    public class DimModule : ModuleBase
    {
        public DimModule() : base("dim")
        {
            LoggerFactory.GetFileLogger().LogCustom("...ctor of dim...");
        }

        public override void Install()
        {
            LoggerFactory.GetFileLogger().LogCustom("...start install of dim...");
            try
            {
                base.Install();
                using (DimSeedDataGenerator generator = new DimSeedDataGenerator())
                {
                    generator.GenerateSeedData();
                }
            }
            catch (Exception e)
            {
                LoggerFactory.GetFileLogger().LogCustom(e.Message);
                LoggerFactory.GetFileLogger().LogCustom(e.StackTrace);
            }

            LoggerFactory.GetFileLogger().LogCustom("...end install of dim...");
        }

        public override void Start()
        {
            base.Start();
            //if (context != null && context.State != null)
            //{
            //    HttpConfiguration config = (HttpConfiguration)this.context.State;
            //    //config.Formatters.Insert(0, new DatasetModelCsvFormatter()); // should also work
            //    config.Formatters.Insert(0, new DatasetModelCsvFormatter(new QueryStringMapping("format", "csv", "text/csv")));
            //}
        }
    }
}