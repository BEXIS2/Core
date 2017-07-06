
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;

namespace BExIS.Modules.Dim.UI.Helpers
{
    public class DimSeedDataGenerator
    {
        public static void GenerateSeedData()
        {
            #region SECURITY
            //workflows = größere sachen, vielen operation
            //operations = einzelne actions

            //1.controller -> 1.Operation


            FeatureManager featureManager = new FeatureManager();

            Feature DataDissemination = featureManager.Create("Data Dissemination", "Data Dissemination");
            Feature Mapping = featureManager.Create("Mapping", "Mapping");
            Feature Submission = featureManager.Create("Submission", "Submission");


            //worklfows -> create dataset ->
            WorkflowManager workflowManager = new WorkflowManager();

            var operation = new Operation();
            Workflow workflow = new Workflow();
            OperationManager operationManager = new OperationManager();


            #region Help Workflow

            workflow = new Workflow();
            workflow.Name = "Data Dissemination Help";
            workflowManager.Create(workflow);

            operation = operationManager.Create("DDM", "Help", "*", null, workflow);
            workflow.Operations.Add(operation);

            DataDissemination.Workflows.Add(workflow);

            #endregion

            #region Admin Workflow

            workflow = new Workflow();
            workflow.Name = "Data Dissemination Management";
            workflowManager.Create(workflow);

            operation = operationManager.Create("Dim", "Admin", "*", null, workflow);
            workflow.Operations.Add(operation);

            DataDissemination.Workflows.Add(workflow);

            #endregion

            #region Mapping Workflow

            //ToDo add security after Refactoring DIM mapping workflow


            //workflow = new Workflow();
            //workflow.Name = "Mapping";
            //workflowManager.Create(workflow);

            //operation = operationManager.Create("Dim", "Admin", "*", null, workflow);
            //workflow.Operations.Add(operation);

            //Mapping.Workflows.Add(workflow);

            #endregion

            #region Submission Workflow

            //ToDo add security after Refactoring DIM Submission workflow

            //workflow = new Workflow();
            //workflow.Name = "Submission";
            //workflowManager.Create(workflow);

            //operation = operationManager.Create("Dim", "Admin", "*", null, workflow);
            //workflow.Operations.Add(operation);

            //Submission.Workflows.Add(workflow);

            #endregion

            #endregion

        }
    }
}