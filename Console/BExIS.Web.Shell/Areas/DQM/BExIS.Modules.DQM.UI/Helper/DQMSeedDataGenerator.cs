using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System;
using System.Linq;

namespace BExIS.Modules.DQM.UI.Helpers
{
    public class DQMSeedDataGenerator : IDisposable
    {
        public void GenerateSeedData()
        {
            using (FeatureManager featureManager = new FeatureManager())
            using (OperationManager operationManager = new OperationManager())
            {
                try
                {
                    Feature DQ =
                    featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Dataset Quality"));
                    if (DQ == null)
                        DQ = featureManager.Create("Dataset Quality", "Dataset Quality");
                    if (!operationManager.Exists("dqm", "ManageDQ", "*")) operationManager.Create("DQM", "ManageDQ", "*", DQ);

                    //operationManager.Create("VIM", "Help", "*");

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
