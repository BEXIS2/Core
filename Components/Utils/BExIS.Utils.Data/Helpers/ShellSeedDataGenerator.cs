using BExIS.Security.Services.Objects;
using System;
using BExIS.Security.Services.Versions;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Utils.Data.Helpers
{
    public class ShellSeedDataGenerator : IModuleSeedDataGenerator
    {
        public void GenerateSeedData()
        {
            // Features
            var featureManager = new FeatureManager();
            var bexisFeature = featureManager.FindByName("BExIS") ?? featureManager.Create("BExIS", "This is the root!");

            // Operations
            var operationManager = new OperationManager();

            var o1 = operationManager.Find("Shell", "Account", "*") ?? operationManager.Create("Shell", "Account", "*");
            var o2 = operationManager.Find("Shell", "Home", "*") ?? operationManager.Create("Shell", "Home", "*");
            var o3 = operationManager.Find("Shell", "Error", "*") ?? operationManager.Create("Shell", "Error", "*");
            var o4 = operationManager.Find("Shell", "TermsAndConditions", "*") ?? operationManager.Create("Shell", "TermsAndConditions", "*");
            var o5 = operationManager.Find("Shell", "PrivacyPolicy", "*") ?? operationManager.Create("Shell", "PrivacyPolicy", "*");
            var o6 = operationManager.Find("Shell", "Footer", "*") ?? operationManager.Create("Shell", "Footer", "*");
            var o7 = operationManager.Find("Shell", "Ldap", "*") ?? operationManager.Create("Shell", "Ldap", "*");
            var o8 = operationManager.Find("Shell", "Help", "*") ?? operationManager.Create("Shell", "Help", "*");

            var versionManager = new VersionManager();
            if (!versionManager.Exists("Shell", "2.14.0"))
            {
                versionManager.Create("Shell", "2.14.0");
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}