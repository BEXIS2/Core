using BExIS.Security.Services.Objects;

namespace BExIS.Web.Shell.Helpers
{
    public class SeedDataGenerator
    {
        public static void Init()
        {
            // Features
            var featureManager = new FeatureManager();
            featureManager.Create("BExIS", "This is the root!");

            // Operations
            var operationManager = new OperationManager();
            operationManager.Create("Shell", "Account", "*");
            operationManager.Create("Shell", "Home", "*");
        }
    }
}