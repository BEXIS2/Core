using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Pum.UI.Helpers
{
    public class PUMSeedDataGenerator : IDisposable
    {
        public void GenerateSeedData()
        {


            // Features
            FeatureManager featureManager = new FeatureManager();
            Feature PumModule =
                    featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Publication Import and View Module"));
                if (PumModule == null)
                    PumModule = featureManager.Create("Publication Import and View Module", "Publication Import and View Module");


            // Operations
            using (OperationManager operationManager = new OperationManager())
            {
                var homeController = operationManager.Find("PUM", "Home", PumModule) ?? operationManager.Create("PUM", "Home", PumModule);
            }

            using (OperationManager operationManager = new OperationManager())
            {
                var importJSONController = operationManager.Find("PUM", "ImportJSON", PumModule) ?? operationManager.Create("PUM", "ImportJSON", PumModule);
            }

            using (OperationManager operationManager = new OperationManager())
            {
                var importCSVController = operationManager.Find("PUM", "ImportCSV", PumModule) ?? operationManager.Create("PUM", "ImportCSV", PumModule);
            }

            using (OperationManager operationManager = new OperationManager())
            {
                var viewController = operationManager.Find("PUM", "View", PumModule) ?? operationManager.Create("PUM", "View", PumModule);
            }


        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}