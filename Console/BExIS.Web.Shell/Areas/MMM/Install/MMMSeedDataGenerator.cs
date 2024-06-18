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
            using (FeatureManager featureManager = new FeatureManager())
            using (FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager())
            using (OperationManager operationManager = new OperationManager())
            {
                List<Feature> features = featureManager.FeatureRepository.Get().ToList();

                Feature Search = features.FirstOrDefault(f => f.Name.Equals("Search"));
                if (Search == null)
                    Search = featureManager.Create("Search", "Search");

                if (!operationManager.Exists("MMM", "ShowMultimediaData", "*"))
                    operationManager.Create("MMM", "ShowMultimediaData", "*");
            }
        }

        public void Dispose()
        {
            // release all the resources
        }
    }
}