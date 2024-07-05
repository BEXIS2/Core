using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System;
using System.Linq;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Vim.UI.Helper
{
    public class VIMSeedDataGenerator : IModuleSeedDataGenerator
    {
        public void GenerateSeedData()
        {
            using (FeatureManager featureManager = new FeatureManager())
            using (OperationManager operationManager = new OperationManager())
            {
                try
                {
                    Feature visualization =
                        featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Visualization"));
                    if (visualization == null)
                        visualization = featureManager.Create("Visualization", "Visualization");

                    operationManager.Create("VIM", "Visualization", "*", visualization);

                    operationManager.Create("VIM", "Help", "*");
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}