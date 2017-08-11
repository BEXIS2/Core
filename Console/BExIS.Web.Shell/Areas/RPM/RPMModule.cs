using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using BExIS.Modules.Rpm.UI.Helpers;
using BExIS.Modules.Rpm.UI.Classes;
using System;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;
using System.Collections.Generic;
using System.Linq;

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

                FeatureManager featureManager = new FeatureManager();
                List<Feature> features = featureManager.FeatureRepository.Get().ToList();

                WorkflowManager workflowManager = new WorkflowManager();

                var operation = new Operation();
                Workflow workflow = new Workflow();
                OperationManager operationManager = new OperationManager();

                List<Workflow> workflows = workflowManager.WorkflowRepository.Get().ToList();
                List<Operation> operations = operationManager.OperationRepository.Get().ToList();

                Feature dataPlanning = features.FirstOrDefault(f => f.Name.Equals("Data Planning"));
                if (dataPlanning == null)
                    dataPlanning = featureManager.Create("Data Planning", "Data Planning Management");

                Feature datastructureFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Datastructure Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (datastructureFeature == null)
                    datastructureFeature = featureManager.Create("Datastructure Management", "Datastructure Management", dataPlanning);

                workflow = workflows.FirstOrDefault(w =>
                    w.Name.Equals("Datastructure Management") &&
                    w.Feature != null &&
                    w.Feature.Id.Equals(datastructureFeature.Id));


                if (workflow == null)
                    workflow = workflowManager.Create("Datastructure Management", "Data Planning Management", datastructureFeature);

                operation = operations.FirstOrDefault(o =>
                    o.Module.Equals("RPM") &&
                    o.Controller.Equals("DataStructureSearch") &&
                    o.Action.Equals("*"));

                if (operation == null)
                    operationManager.Create("RPM", "DataStructureSearch", "*", null, workflow);

                operation = operations.FirstOrDefault(o =>
                    o.Module.Equals("RPM") &&
                    o.Controller.Equals("DataStructureEdit") &&
                    o.Action.Equals("*"));

                if (operation == null)
                    operationManager.Create("RPM", "DataStructureEdit", "*", null, workflow);

                operation = operations.FirstOrDefault(o =>
                    o.Module.Equals("RPM") &&
                    o.Controller.Equals("DataStructureIO") &&
                    o.Action.Equals("*"));

                if (operation == null)
                    operationManager.Create("RPM", "DataStructureIO", "*", null, workflow);

                operation = operations.FirstOrDefault(o =>
                   o.Module.Equals("RPM") &&
                   o.Controller.Equals("Structures") &&
                   o.Action.Equals("*"));

                if (operation == null)
                    operationManager.Create("RPM", "Structures", "*", null, workflow);


                Feature atributeFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Variable Template Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (atributeFeature == null)
                    atributeFeature = featureManager.Create("Variable Template Management", "Variable Template Management", dataPlanning);

                workflow = workflows.FirstOrDefault(w =>
                    w.Name.Equals("Variable Template Management") &&
                    w.Feature != null &&
                    w.Feature.Id.Equals(atributeFeature.Id));


                if (workflow == null)
                    workflow = workflowManager.Create("Variable Template Management", "Data Planning Management", atributeFeature);

                operation = operations.FirstOrDefault(o =>
                    o.Module.Equals("RPM") &&
                    o.Controller.Equals("DataAttribute") &&
                    o.Action.Equals("*"));

                if (operation == null)
                    operationManager.Create("RPM", "DataAttribute", "*", null, workflow);

                Feature unitFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Unit Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (unitFeature == null)
                    unitFeature = featureManager.Create("Unit Management", "Unit Management", dataPlanning);

                workflow = workflows.FirstOrDefault(w =>
                    w.Name.Equals("Unit Management") &&
                    w.Feature != null &&
                    w.Feature.Id.Equals(unitFeature.Id));

                if (workflow == null)
                    workflow = workflowManager.Create("Unit Management", "Data Planning Management", unitFeature);

                operation = operations.FirstOrDefault(o =>
                    o.Module.Equals("RPM") &&
                    o.Controller.Equals("Unit") &&
                    o.Action.Equals("*"));

                if (operation == null)
                    operationManager.Create("RPM", "Unit", "*", null, workflow);                

                Feature dataTypeFeature = features.FirstOrDefault(f =>
                    f.Name.Equals("Data Type Management") &&
                    f.Parent != null &&
                    f.Parent.Id.Equals(dataPlanning.Id));

                if (dataTypeFeature == null)
                    dataTypeFeature = featureManager.Create("Data Type Management", "Data Type Management", dataPlanning);

                workflow = workflows.FirstOrDefault(w =>
                    w.Name.Equals("Data Type Management") &&
                    w.Feature != null &&
                    w.Feature.Id.Equals(dataTypeFeature.Id));

                if (workflow == null)
                    workflow = workflowManager.Create("Data Type Management", "Data Planning Management", dataTypeFeature);

                operation = operations.FirstOrDefault(o =>
                    o.Module.Equals("RPM") &&
                    o.Controller.Equals("Home") &&
                    o.Action.Equals("*"));

                if (operation == null)
                    operationManager.Create("RPM", "Home", "*", null, workflow);
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
