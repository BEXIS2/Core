using BExIS.Modules.DQM.UI.Helpers;
using System;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.DQM.UI
{
    public class DQMModule : ModuleBase
    {
        public DQMModule() : base("DQM")
        {
            LoggerFactory.GetFileLogger().LogCustom("...ctor of DQM...");
        }
        public override void Install()
        {
            LoggerFactory.GetFileLogger().LogCustom("...start install of DQM...");
            try
            {
                base.Install();
                using (DQMSeedDataGenerator generator = new DQMSeedDataGenerator())
                {
                    generator.GenerateSeedData();
                }

            }
            catch (Exception e)
            {
                LoggerFactory.GetFileLogger().LogCustom(e.Message);
                LoggerFactory.GetFileLogger().LogCustom(e.StackTrace);
            }

            LoggerFactory.GetFileLogger().LogCustom("...end install of DQM...");

        }
        /// <summary>
        /// Registers current area with the routing engine.
        /// The default route is automatically registred. Using the AreaName as route prefix and url sapce.
        /// </summary>
        /// <remarks>
        /// <list type="number">
        ///     <item>If you are happy with the defaul route, either leave the method as is or comment it all (prefered).</item>
        ///     <item>if you want to register other than the default, comment the call to the base method and write your own ones.</item>
        ///     <item>If you want to register additional routes, write them after the call to the base method.</item>
        /// </list>
        /// </remarks>
        /// <param name="context"></param>
        //public override void RegisterArea(AreaRegistrationContext context)
        //{
        //    base.RegisterArea(context);
        //    //context.MapRoute(
        //    //    AreaName + "_default",
        //    //    AreaName+"/{controller}/{action}/{id}",
        //    //    new { action = "Index", id = UrlParameter.Optional }
        //    //);
        //}
    }
}
