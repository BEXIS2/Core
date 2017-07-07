
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System.Linq;

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

            Feature DataDissemination = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Data Dissemination"));
            if (DataDissemination == null) DataDissemination = featureManager.Create("Data Dissemination", "Data Dissemination");

            Feature Mapping = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Mapping"));
            if (Mapping == null) Mapping = featureManager.Create("Mapping", "Mapping");

            Feature Submission = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Submission"));
            if (Submission == null) Submission = featureManager.Create("Submission", "Submission");


            //worklfows -> create dataset ->
            WorkflowManager workflowManager = new WorkflowManager();

            var operation = new Operation();
            Workflow workflow = new Workflow();
            OperationManager operationManager = new OperationManager();


            #region Help Workflow

            workflow =
                workflowManager.WorkflowRepository
                    .Get()
                    .FirstOrDefault(w => w.Name.Equals("Data Dissemination Help") && w.Feature.Id.Equals(DataDissemination.Id));
            if (workflow == null) workflow = workflowManager.Create("Data Dissemination Help", "", DataDissemination);

            operationManager.Create("DIM", "Help", "*", null, workflow);

            #endregion

            #region Admin Workflow

            workflow =
               workflowManager.WorkflowRepository
                   .Get()
                   .FirstOrDefault(w => w.Name.Equals("Data Dissemination Management") && w.Feature.Id.Equals(DataDissemination.Id));

            if (workflow == null) workflow = workflowManager.Create("Data Dissemination Management", "", DataDissemination);

            operationManager.Create("Dim", "Admin", "*", null, workflow);


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