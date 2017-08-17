using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.Rpm.UI.Classes;
using BExIS.Modules.Rpm.UI.Helpers;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
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
                foreach (StructuredDataStructure sds in dsm.StructuredDataStructureRepo.Get())
                {
                    DataStructureIO.convertOrder(sds);
                }

                FeatureManager featureManager = new FeatureManager();
                List<Feature> features = featureManager.FeatureRepository.Get().ToList();

                OperationManager operationManager = new OperationManager();

                Feature dataPlanning = features.FirstOrDefault(f => f.Name.Equals("Data Planning"));
                if (dataPlanning == null)
                    dataPlanning = featureManager.Create("Data Planning", "Data Planning Management");

                Feature datastructureFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Datastructure Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (datastructureFeature == null)
                    datastructureFeature = featureManager.Create("Datastructure Management", "Datastructure Management", dataPlanning);

                if (!operationManager.Exists("RPM", "DataStructureSearch", "*"))
                    operationManager.Create("RPM", "DataStructureSearch", "*", datastructureFeature);

                if (!operationManager.Exists("RPM", "DataStructureEdit", "*"))
                    operationManager.Create("RPM", "DataStructureEdit", "*", datastructureFeature);

                if (!operationManager.Exists("RPM", "Structures", "*"))
                    operationManager.Create("RPM", "Structures", " * ", datastructureFeature);

                Feature atributeFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Variable Template Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (atributeFeature == null)
                    atributeFeature = featureManager.Create("Variable Template Management", "Variable Template Management", dataPlanning); ;

                if (!operationManager.Exists("RPM", "DataAttribute", "*"))
                    operationManager.Create("RPM", "DataAttribute", " * ", atributeFeature);

                Feature unitFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Unit Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (unitFeature == null)
                    unitFeature = featureManager.Create("Unit Management", "Unit Management", dataPlanning);

                if (!operationManager.Exists("RPM", "Unit", "*"))
                    operationManager.Create("RPM", "Unit", " * ", unitFeature);

                Feature dataTypeFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Data Type Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (dataTypeFeature == null)
                    dataTypeFeature = featureManager.Create("Data Type Management", "Data Type Management", dataPlanning);

                if (!operationManager.Exists("RPM", "Home", "*"))
                    operationManager.Create("RPM", "Home", " * ", dataTypeFeature);
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
