using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System.Linq;

namespace BExIS.Modules.Sam.UI.Helpers
{
    public class SamSeedDataGenerator
    {
        public static void GenerateSeedData()
        {
            createSecuritySeedData();
        }

        private static void createSecuritySeedData()
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
            var operationManager = new OperationManager();
            var featureManager = new FeatureManager();

            var root = featureManager.FindRoots().FirstOrDefault();

            var administrationFeature = featureManager.Create("Administration", "node for all administrative features", root);

            var userFeature = featureManager.Create("Users", "", administrationFeature);
            var userOperation = operationManager.Create("SAM", "Users", "*", userFeature);

            var groupFeature = featureManager.Create("Groups", "", administrationFeature);
            var groupOperation = operationManager.Create("SAM", "Groups", "*", groupFeature);

            var featurePermissionFeature = featureManager.Create("Feature Permissions", "", administrationFeature);
            var featurePermissionOperation = operationManager.Create("SAM", "FeaturePermissions", "*", featurePermissionFeature);

            var entityPermissionFeature = featureManager.Create("Entity Permissions", "", administrationFeature);
            var entityPermissionOperation = operationManager.Create("SAM", "EntityPermissions", "*", entityPermissionFeature);

            var featurePermissionManager = new FeaturePermissionManager();
            featurePermissionManager.Create(null, featurePermissionFeature, PermissionType.Grant);
        }
    }
}