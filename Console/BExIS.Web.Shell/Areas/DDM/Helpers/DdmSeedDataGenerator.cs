
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System.Linq;

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


            Feature DataDiscovery = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Data Discovery"));
            if (DataDiscovery == null) DataDiscovery = featureManager.Create("Data Discovery", "Data Discovery");

            Feature SearchFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Search") && f.Parent.Id.Equals(DataDiscovery.Id));
            if (SearchFeature == null) SearchFeature = featureManager.Create("Search", "Search");

            Feature SearchManagementFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Search Management") && f.Parent.Id.Equals(DataDiscovery.Id));
            if (SearchManagementFeature == null) SearchManagementFeature = featureManager.Create("Search Management", "Search Management");



            //worklfows -> create dataset ->
            WorkflowManager workflowManager = new WorkflowManager();

            var operation = new Operation();
            Workflow workflow = new Workflow();
            OperationManager operationManager = new OperationManager();


            #region Help Workflow

            workflow =
                workflowManager.WorkflowRepository
                    .Get()
                    .FirstOrDefault(w => w.Name.Equals("Search Help") && w.Feature.Id.Equals(DataDiscovery.Id));
            if (workflow == null) workflow = workflowManager.Create("Search Help", "", DataDiscovery);

            operationManager.Create("DDM", "Help", "*", null, workflow);

            #endregion

            #region Search Workflow

            workflow =
                workflowManager.WorkflowRepository
                    .Get()
                    .FirstOrDefault(w => w.Name.Equals("Search") && w.Feature.Id.Equals(SearchFeature.Id));
            if (workflow == null) workflow = workflowManager.Create("Search", "", SearchFeature);

            operationManager.Create("DDM", "Home", "*", null, workflow);
            operationManager.Create("DDM", "Data", "*", null, workflow);

            #endregion

            #region Search Admin Workflow

            workflow =
                workflowManager.WorkflowRepository
                    .Get()
                    .FirstOrDefault(w => w.Name.Equals("Search Managment") && w.Feature.Id.Equals(SearchManagementFeature.Id));
            if (workflow == null) workflow = workflowManager.Create("Search", "", SearchManagementFeature);

            operationManager.Create("DDM", "Admin", "*", null, workflow);

            #endregion


            #endregion

        }
    }
}