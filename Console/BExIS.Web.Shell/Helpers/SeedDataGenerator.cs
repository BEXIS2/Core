using BExIS.Security.Services.Objects;

namespace BExIS.Web.Shell.Helpers
{
    public class SeedDataGenerator
    {
        public static void Init()
        {
            // Features
            var featureManager = new FeatureManager();
            var bexisFeature = featureManager.FindByName("BExIS") ?? featureManager.Create("BExIS", "This is the root!");

            // Operations
            var operationManager = new OperationManager();

            var o1 = operationManager.Find("Shell", "Account", "*") ?? operationManager.Create("Shell", "Account", "*");
            var o2 = operationManager.Find("Shell", "Home", "*") ?? operationManager.Create("Shell", "Home", "*");
            var o3 = operationManager.Find("Shell", "Error", "*") ?? operationManager.Create("Shell", "Error", "*");
            var o4 = operationManager.Find("Shell", "Terms", "*") ?? operationManager.Create("Shell", "Terms", "*");
        }
    }
}