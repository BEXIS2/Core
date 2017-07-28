
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System.Collections.Generic;
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
            List<Feature> features = featureManager.FeatureRepository.Get().ToList();

            Feature DataDiscovery = features.FirstOrDefault(f => f.Name.Equals("Data Discovery"));
            if (DataDiscovery == null) DataDiscovery = featureManager.Create("Data Discovery", "Data Discovery");

            Feature SearchFeature = features.FirstOrDefault(f =>
                f.Name.Equals("Search") &&
                f.Parent != null &&
                f.Parent.Id.Equals(DataDiscovery.Id));

            if (SearchFeature == null) SearchFeature = featureManager.Create("Search", "Search", DataDiscovery);

            Feature SearchManagementFeature = features.FirstOrDefault(f =>
                f.Name.Equals("Search Managment") &&
                f.Parent != null &&
                f.Parent.Id.Equals(DataDiscovery.Id));

            if (SearchManagementFeature == null) SearchManagementFeature = featureManager.Create("Search Management", "Search Management", DataDiscovery);



            //worklfows -> create dataset ->
            WorkflowManager workflowManager = new WorkflowManager();

            var operation = new Operation();
            Workflow workflow = new Workflow();
            OperationManager operationManager = new OperationManager();

            List<Workflow> workflows = workflowManager.WorkflowRepository.Get().ToList();

            #region Help Workflow

            workflow =
                workflows.FirstOrDefault(w =>
                w.Name.Equals("Search Help") &&
                w.Feature != null &&
                w.Feature.Id.Equals(DataDiscovery.Id));

            if (workflow == null) workflow = workflowManager.Create("Search Help", "", DataDiscovery);

            operationManager.Create("DDM", "Help", "*", null, workflow);

            #endregion

            #region Search Workflow

            workflow = workflows.FirstOrDefault(w =>
                w.Name.Equals("Search") &&
                w.Feature != null &&
                w.Feature.Id.Equals(SearchFeature.Id));


            if (workflow == null) workflow = workflowManager.Create("Search", "", SearchFeature);

            operationManager.Create("DDM", "Home", "*", null, workflow);
            operationManager.Create("DDM", "Data", "*", null, workflow);

            #endregion

            #region Search Admin Workflow

            workflow = workflows.FirstOrDefault(w =>
                w.Name.Equals("Search Managment") &&
                w.Feature != null &&
                w.Feature.Id.Equals(SearchManagementFeature.Id));

            if (workflow == null) workflow = workflowManager.Create("Search", "", SearchManagementFeature);

            operationManager.Create("DDM", "Admin", "*", null, workflow);

            #endregion


            #endregion

        }
    }
}