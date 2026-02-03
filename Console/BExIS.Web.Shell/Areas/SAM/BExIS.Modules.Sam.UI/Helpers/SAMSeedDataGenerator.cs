using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System.Linq;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Sam.UI.Helpers
{
    public class SamSeedDataGenerator : IModuleSeedDataGenerator
    {
        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void GenerateSeedData()
        {
            // Javad:
            // 1) all the create operations should check for existence of the record
            // 2) failure on creating any record should rollback the whole seed data generation. It is one transaction.
            // 3) failues should throw an exception with enough information to pin point the root cause
            // 4) only seed data related to the functions of this modules should be genereated here.
            // BUG: seed data creation is not working because of the changes that were done in the entities and services.
            // TODO: reimplement the seed data creation method.

            //#region Security

            //// Tasks
            ///
            var featureManager = new FeatureManager();

            var operationManager = new OperationManager();
            var featurePermissionManager = new FeaturePermissionManager();
            // find root
            var root = featureManager.FindRoots().FirstOrDefault();

            // administration node
            var administrationFeature = featureManager.GetByName("Administration") ?? featureManager.Create("Administration", "node for all administrative features", root);

            // users node
            var userFeature = featureManager.GetByName("Users") ?? featureManager.Create("Users", "", administrationFeature);
            var userOperation = operationManager.Get("SAM", "Users", "*") ?? operationManager.Create("SAM", "Users", "*", userFeature);

            // groups node
            var groupFeature = featureManager.GetByName("Groups") ?? featureManager.Create("Groups", "", administrationFeature);
            var groupOperation = operationManager.Get("SAM", "Groups", "*") ?? operationManager.Create("SAM", "Groups", "*", groupFeature);

            // feature permissions
            var featurePermissionFeature = featureManager.GetByName("Feature Permissions") ?? featureManager.Create("Feature Permissions", "", administrationFeature);
            var featurePermissionOperation = operationManager.Get("SAM", "FeaturePermissions", "*") ?? operationManager.Create("SAM", "FeaturePermissions", "*", featurePermissionFeature);

            // Entity Permissions
            var entityPermissionFeature = featureManager.GetByName("Entity Permissions") ?? featureManager.Create("Entity Permissions", "", administrationFeature);
            var entityPermissionOperation = operationManager.Get("SAM", "EntityPermissions", "*") ?? operationManager.Create("SAM", "EntityPermissions", "*", entityPermissionFeature);

            // User Permissions
            var userPermissionFeature = featureManager.GetByName("User Permissions") ?? featureManager.Create("User Permissions", "", administrationFeature);
            var userPermissionOperation = operationManager.Get("SAM", "UserPermissions", "*") ?? operationManager.Create("SAM", "UserPermissions", "*", userPermissionFeature);

            // Dataset Management
            var datasetManagementFeature = featureManager.GetByName("Dataset Management") ?? featureManager.Create("Dataset Management", "", administrationFeature);
            var datasetManagementOperation = operationManager.Get("SAM", "Datasets", "*") ?? operationManager.Create("SAM", "Datasets", "*", datasetManagementFeature);

            // Document Management
            var documentManagementFeature = featureManager.GetByName("Document Management") ?? featureManager.Create("Document Management", "", administrationFeature);
            var documentManagementOperation = operationManager.Get("SAM", "Files", "*") ?? operationManager.Create("SAM", "Files", "*", documentManagementFeature);

            // Request
            var requestManagementFeature = featureManager.GetByName("Request Management") ?? featureManager.Create("Request Management", "", administrationFeature);
            var requestManagementOperation = operationManager.Get("SAM", "RequestsAdmin", "*") ?? operationManager.Create("SAM", "RequestsAdmin", "*", requestManagementFeature);

            var requestOperation = operationManager.Get("SAM", "Requests", "*") ?? operationManager.Create("SAM", "Requests", "*");

            // Help
            var helpOperation = operationManager.Get("SAM", "Help", "*") ?? operationManager.Create("SAM", "Help", "*");

            // Formeer Member
            var formerMemberFeature = featureManager.Find(f => f.Name.Equals("Former Member Management")).SingleOrDefault();
            if (formerMemberFeature == null) formerMemberFeature = featureManager.Create("Former Member Management", "Former Member Management", administrationFeature);
            operationManager.Create("SAM", "FormerMember", "*", formerMemberFeature);

            if (!featurePermissionManager.Exists(null, featurePermissionFeature.Id, PermissionType.Grant))
            {
                var result_create = featurePermissionManager.Create(null, featurePermissionFeature.Id, PermissionType.Grant);
            }
        }
    }
}