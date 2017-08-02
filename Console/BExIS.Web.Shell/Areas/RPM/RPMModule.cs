using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.Rpm.UI.Helpers;
using BExIS.Modules.Rpm.UI.Classes;
using System;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Rpm.UI
{
    public class RpmModule : ModuleBase
    {
        public RpmModule() : base("RPM")
        {
            LoggerFactory.GetFileLogger().LogCustom("...ctor of rpm...");
        }

        public override void Install()
        {
            LoggerFactory.GetFileLogger().LogCustom("...start install of rpm...");
            try
            {
                base.Install();
                RPMSeedDataGenerator.GenerateSeedData();
                DataStructureManager dsm = new DataStructureManager();
                foreach(StructuredDataStructure sds in dsm.StructuredDataStructureRepo.Get())
                {
                    DataStructureIO.convertOrder(sds);
                }
            }
            catch (Exception e)
            {
                LoggerFactory.GetFileLogger().LogCustom(e.Message);
                LoggerFactory.GetFileLogger().LogCustom(e.StackTrace);
            }

            LoggerFactory.GetFileLogger().LogCustom("...end install of rpm...");


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
