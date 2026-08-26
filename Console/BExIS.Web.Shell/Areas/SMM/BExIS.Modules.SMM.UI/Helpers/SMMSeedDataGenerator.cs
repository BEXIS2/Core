using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System;
using System.Linq;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Smm.UI.Helpers
{
    public class SMMSeedDataGenerator : IDisposable
    {
        public void GenerateSeedData()
        {
            FeatureManager featureManager = new FeatureManager();
            OperationManager operationManager = new OperationManager();

            try
            {
                #region SECURITY

                Feature speciesMatchingFeature = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Species Matching"));
                if (speciesMatchingFeature == null) speciesMatchingFeature = featureManager.Create("Species Matching", "Species matching and taxonomic validation");

                operationManager.Create("SMM", "Home", "*");
                operationManager.Create("SMM", "Species", "*", speciesMatchingFeature);

                #endregion SECURITY
            }
            finally
            {
                featureManager.Dispose();
                operationManager.Dispose();
            }
        }

        public void Dispose()
        {
        }
    }
}
