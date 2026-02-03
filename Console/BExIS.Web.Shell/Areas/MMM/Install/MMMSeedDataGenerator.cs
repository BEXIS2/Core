using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Mmm.UI.Helpers
{
    public class MMMSeedDataGenerator : IModuleSeedDataGenerator
    {
        public void GenerateSeedData()
        {
            var featureManager = new FeatureManager();
            var operationManager = new OperationManager();
            var featurePermissionManager = new FeaturePermissionManager();

            Feature Search = featureManager.GetByName("Search");
            if (Search == null)
                Search = featureManager.Create("Search", "Search");

            if (!operationManager.Exists("MMM", "ShowMultimediaData", "*"))
                operationManager.Create("MMM", "ShowMultimediaData", "*");
        }

        public void Dispose()
        {
            // release all the resources
        }
    }
}