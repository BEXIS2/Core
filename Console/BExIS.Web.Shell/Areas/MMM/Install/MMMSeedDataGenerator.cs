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

            FeatureManager featureManager = null;
            FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager();

            try
            {
                featureManager = new FeatureManager();
                List<Feature> features = featureManager.FeatureRepository.Get().ToList();

                OperationManager operationManager = new OperationManager();

                Feature Search = features.FirstOrDefault(f => f.Name.Equals("Search"));
                if (Search == null)
                    Search = featureManager.Create("Search", "Search");          
                
                if (!operationManager.Exists("MMM", "ShowMultimediaData", "*"))
                    operationManager.Create("MMM", "ShowMultimediaData", "*", Search);

                

            }
            finally
            {
                featureManager.Dispose();
                featurePermissionManager.Dispose();
            }
        }

        public void Dispose()
        {
            // release all the resources
        }
    }
}