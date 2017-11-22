using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
using System.Linq;

namespace BExIS.Modules.Sam.UI.Helpers
{
    public class SamSeedDataGenerator : IDisposable
    {
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
            OperationManager operationManager = null;
            FeatureManager featureManager = null;

            try
            {
                operationManager = new OperationManager();
                featureManager = new FeatureManager();

                // find root
                var root = featureManager.FindRoots().FirstOrDefault();

                // administration node
                var administrationFeature = featureManager.FindByName("Administration") ?? featureManager.Create("Administration", "node for all administrative features", root);

                // users node
                var userFeature = featureManager.FindByName("Users") ?? featureManager.Create("Users", "", administrationFeature);
                var userOperation = operationManager.Find("SAM", "Users", "*") ?? operationManager.Create("SAM", "Users", "*", userFeature);

                // groups node
                var groupFeature = featureManager.FindByName("Groups") ?? featureManager.Create("Groups", "", administrationFeature);
                var groupOperation = operationManager.Find("SAM", "Groups", "*") ?? operationManager.Create("SAM", "Groups", "*", groupFeature);

                // feature permissions
                var featurePermissionFeature = featureManager.FindByName("Feature Permissions") ?? featureManager.Create("Feature Permissions", "", administrationFeature);
                var featurePermissionOperation = operationManager.Find("SAM", "FeaturePermissions", "*") ?? operationManager.Create("SAM", "FeaturePermissions", "*", featurePermissionFeature);

                // Entity Permissions
                var entityPermissionFeature = featureManager.FindByName("Entity Permissions") ?? featureManager.Create("Entity Permissions", "", administrationFeature);
                var entityPermissionOperation = operationManager.Find("SAM", "EntityPermissions", "*") ?? operationManager.Create("SAM", "EntityPermissions", "*", entityPermissionFeature);

                // User Permissions
                var userPermissionFeature = featureManager.FindByName("User Permissions") ?? featureManager.Create("User Permissions", "", administrationFeature);
                var userPermissionOperation = operationManager.Find("SAM", "UserPermissions", "*") ?? operationManager.Create("SAM", "UserPermissions", "*", userPermissionFeature);


                // Dataset Management
                var datasetManagementFeature = featureManager.FindByName("Dataset Management") ?? featureManager.Create("Dataset Management", "", administrationFeature);
                var datasetManagementOperation = operationManager.Find("SAM", "Datasets", "*") ?? operationManager.Create("SAM", "Datasets", "*", datasetManagementFeature);

                // Help
                var helpOperation = operationManager.Find("SAM", "Help", "*") ?? operationManager.Create("SAM", "Help", "*");

                var featurePermissionManager = new FeaturePermissionManager();

                if (!featurePermissionManager.Exists(null, featurePermissionFeature.Id, PermissionType.Grant))
                    featurePermissionManager.Create(null, featurePermissionFeature.Id, PermissionType.Grant);
            }
            finally
            {
                featureManager?.Dispose();
                operationManager?.Dispose();
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}