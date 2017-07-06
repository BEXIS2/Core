
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;

namespace BExIS.Modules.Ddm.UI.Helpers
{
    public class DdmSeedDataGenerator
    {
        public static void GenerateSeedData()
        {
            #region SECURITY
            //workflows = größere sachen, vielen operation
            //operations = einzelne actions

            //1.controller -> 1.Operation


            FeatureManager featureManager = new FeatureManager();

            Feature DataDiscovery = featureManager.Create("Search", "Search");


            Feature SearchFeature = featureManager.Create("Search", "Search", DataDiscovery);
            Feature SearchManagementFeature = featureManager.Create("Search Management", "Search Management", DataDiscovery);

            //worklfows -> create dataset ->
            WorkflowManager workflowManager = new WorkflowManager();

            var operation = new Operation();
            Workflow workflow = new Workflow();
            OperationManager operationManager = new OperationManager();


            #region Help Workflow

            workflow = new Workflow();
            workflow.Name = "Search Help";
            workflowManager.Create(workflow);

            operation = operationManager.Create("DDM", "Help", "*", null, workflow);
            workflow.Operations.Add(operation);

            DataDiscovery.Workflows.Add(workflow);

            #endregion

            #region Search Workflow

            workflow = new Workflow();
            workflow.Name = "Search";
            workflowManager.Create(workflow);

            operation = operationManager.Create("DDM", "Home", "*", null, workflow);
            workflow.Operations.Add(operation);
            operation = operationManager.Create("DDM", "Data", "*", null, workflow);
            workflow.Operations.Add(operation);
            SearchFeature.Workflows.Add(workflow);

            #endregion

            #region Search Admin Workflow

            workflow = new Workflow();
            workflow.Name = "Search Managment";
            workflowManager.Create(workflow);

            operation = operationManager.Create("DDM", "Admin", "*", null, workflow);
            workflow.Operations.Add(operation);

            SearchManagementFeature.Workflows.Add(workflow);

            #endregion


            #endregion

        }
    }
}