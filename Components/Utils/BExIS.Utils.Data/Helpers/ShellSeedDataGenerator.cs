using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Versions;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Utils.Data.Helpers
{
    public class ShellSeedDataGenerator : IModuleSeedDataGenerator
    {
        public void Dispose()
        {
            // throw new NotImplementedException();
        }

        public void GenerateSeedData()
        {
            // Features
            var featureManager = new FeatureManager();
            var operationManager = new OperationManager();
            var versionManager = new VersionManager();
            var bexisFeature = featureManager.GetByName("BExIS") ?? featureManager.Create("BExIS", "This is the root!");
            var settings = featureManager.GetByName("Settings") ?? featureManager.Create("Settings", "This is the settings page!", bexisFeature);

            // Operations
            var o1 = operationManager.Get("Shell", "Account", "*") ?? operationManager.Create("Shell", "Account", "*");
            var o2 = operationManager.Get("Shell", "Home", "*") ?? operationManager.Create("Shell", "Home", "*");
            var o3 = operationManager.Get("Shell", "Error", "*") ?? operationManager.Create("Shell", "Error", "*");
            var o4 = operationManager.Get("Shell", "TermsAndConditions", "*") ?? operationManager.Create("Shell", "TermsAndConditions", "*");
            var o5 = operationManager.Get("Shell", "PrivacyPolicy", "*") ?? operationManager.Create("Shell", "PrivacyPolicy", "*");
            var o99 = operationManager.Get("Shell", "Header", "*") ?? operationManager.Create("Shell", "Header", "*");
            var o6 = operationManager.Get("Shell", "Footer", "*") ?? operationManager.Create("Shell", "Footer", "*");
            var o7 = operationManager.Get("Shell", "Ldap", "*") ?? operationManager.Create("Shell", "Ldap", "*");
            var o8 = operationManager.Get("Shell", "Help", "*") ?? operationManager.Create("Shell", "Help", "*");

            var o9 = operationManager.Get("Api", "Tokens", "*") ?? operationManager.Create("Api", "Tokens", "*");
            var o98 = operationManager.Get("Shell", "Tokens", "*") ?? operationManager.Create("Shell", "Tokens", "*");

            var o10 = operationManager.Get("Shell", "Menu", "*") ?? operationManager.Create("Shell", "Menu", "*");
            var o11 = operationManager.Get("Shell", "UiTest", "*") ?? operationManager.Create("Shell", "Help", "*");

            var o12 = operationManager.Get("Shell", "Settings", "*") ?? operationManager.Create("Shell", "Settings", "*", settings);

            if (!versionManager.Exists("Shell", "4.2.1"))
            {
                versionManager.Create("Shell", "4.2.1");
            }
        }
    }
}